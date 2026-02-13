using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using $safeprojectname$.Resources;

namespace $safeprojectname$.Service
{
    class ServiceProcess : ServiceBase
    {
        private readonly string[] _arguments;
        private IsolationContext _context;
        private ServiceImplementation _service;

        public ServiceProcess(IEnumerable<string> arguments)
        {
            _arguments = new List<string>(arguments).ToArray();
            if (_arguments.Length == 0 || String.IsNullOrEmpty(_arguments[0]))
                throw new ArgumentException("Required parameter service name not provided.");

            AutoLog = false;
            CanStop = true;
            CanShutdown = true;
            CanHandlePowerEvent = true;
            CanHandleSessionChangeEvent = true;
            CanPauseAndContinue = typeof(IPauseAndContinue).IsAssignableFrom(typeof(ServiceImplementation));

            ServiceName = _arguments[0];
        }

        protected override void Dispose(bool disposing)
        {
            if (_service != null)
                _service.Dispose();
            _service = null;

            if (_context != null)
                _context.Dispose();
            _context = null;

            base.Dispose(disposing);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                OnStop();
                List<string> allarguments = new List<string>(_arguments);
                if (args != null && args.Length > 0)
                    allarguments.AddRange(args);
#if DEBUG
                if (allarguments.Remove("DEBUG!"))
                    System.Diagnostics.Debugger.Break();
#endif
                _context = new IsolationContext(ServiceName);
                _service = _context.CreateInstance<ServiceImplementation>(ServiceName);

                args = allarguments.ToArray();
                _service.Start(args);

                Messages.ServiceStarted(ServiceName);
            }
            catch(Exception e)
            {
                try
                {
                    if (_service != null)
                        _service.Dispose();
                    if (_context != null)
                        _context.Dispose();
                }
                catch (Exception ex2)
                { Messages.Warning(ServiceName, ex2.Message, ex2); }
                finally
                {
                    _service = null; 
                    _context = null;
                }

                throw new ServiceStartException(ServiceName, e);
            }
        }

        protected override void OnStop()
        {
            if (_service == null)
                return;

            try
            {
                using (_context)
                using (_service)
                    _service.Stop();
                
                Messages.ServiceStopped(ServiceName);
            }
            catch (Exception e)
            {
                throw new ServiceStopException(ServiceName, e);
            }
            finally
            {
                _context = null;
                _service = null;
            }
        }

        protected override void OnShutdown()
        {
            if (_service == null)
                return;

            try
            {
                using (_context)
                using (_service)
                    _service.Shutdown();

                Messages.ServiceStopped(ServiceName);
            }
            catch (Exception e)
            {
                throw new ServiceStopException(ServiceName, e);
            }
            finally
            {
                _context = null;
                _service = null;
            }
        }

        protected override void OnPause()
        {
            if (_service == null)
                return;
            try
            {
                ((IPauseAndContinue) _service).Pause();
                Messages.ServicePaused(ServiceName);
            }
            catch(Exception e)
            {
                throw new ServicePauseException(ServiceName, e);
            }
        }

        protected override void OnContinue()
        {
            try
            {
                ((IPauseAndContinue) _service).Continue();
                Messages.ServiceResumed(ServiceName);
            }
            catch (Exception e)
            {
                throw new ServiceContinueException(ServiceName, e);
            }
        }

        protected override void OnCustomCommand(int command)
        {
            if (_service == null)
                return;

            try
            {
                Messages.ServiceCommand(ServiceName, command);
                _service.ExecuteCommand(command);

                using (EventWaitHandle successSignal = new EventWaitHandle(false, EventResetMode.ManualReset, 
                    FormatString.CommandSuccessSignal(ServiceName, command)))
                    successSignal.Set();
            }
            catch (Exception e)
            {
                try
                {
                    using (EventWaitHandle failureSignal = new EventWaitHandle(false, EventResetMode.ManualReset,
                        FormatString.CommandFailureSignal(ServiceName, command)))
                        failureSignal.Set();
                }
                finally
                {
                    throw new ServiceCommandException(ServiceName, command, e);
                }
            }
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            if (_service == null)
                return true;

            try
            {
                if(powerStatus == PowerBroadcastStatus.QuerySuspend)
                {
                    if (!_service.CanSuspend())
                        return false;
                }
                else
                    _service.PowerEvent(powerStatus);
            }
            catch (Exception e)
            {
                Messages.ServicePowerEventError(ServiceName, powerStatus, e.Message, e);
            }
            return true;
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            if (_service == null)
                return;

            try
            {
                _service.SessionChange(changeDescription.Reason, changeDescription.SessionId);
            }
            catch (Exception e)
            {
                Messages.ServiceSessionChangeError(ServiceName, 
                    changeDescription.Reason, changeDescription.SessionId, e.Message, e);
            }
        }
    }
}
