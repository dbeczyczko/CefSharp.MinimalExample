using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using e2Controls.Forms;
using System.Reflection;
using System.Diagnostics;
using System.Drawing;
using System.ComponentModel;

namespace e2Controls
{
    public static class e2Forms
    {
        private static List<e2WindowForm> _delayedDisplayForms = new List<e2WindowForm>();
        private static List<e2IdleEntry> _pendingDelegates = new List<e2IdleEntry>();

        public static void RegisterDelayedDisplayForm(e2WindowForm form)
        {
            return;
            
        }

        public static void ApplicationIdleRoutines()
        {
            try
            {
                DisplayWaitingForms();

                lock (_pendingDelegates)
                {
                    _pendingDelegates.Sort();
                    while (_pendingDelegates.Count > 0)
                    {
                        e2IdleEntry dg = _pendingDelegates[0];
                        _pendingDelegates.RemoveAt(0);

                        if (Debugger.IsAttached)
                        {
                            dg.Delegate.DynamicInvoke();
                        }
                        else
                        {
                            try
                            {
                                dg.Delegate.DynamicInvoke();
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceWarning("IDLE: " + ex.ToString() + "\r\n" + dg.QueueStackTrace.ToString());
                            }
                        }

                        _pendingDelegates.Sort();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("IDLE: " + ex.ToString());
            }
        }

        public static void AddIdleDelegateBlock(MethodInvoker action)
        {
            AddIdleDelegateInternal(action, _pendingDelegates.Count);
        }

        public static void AddIdleDelegate(MethodInvoker action)
        {
            AddIdleDelegateInternal(new MethodInvoker(delegate()
            {
                action.Invoke();
            }), _pendingDelegates.Count);   
        }

        public static void AddIdleDelegateIgnoreReturn<T>(e2IdleDelegate<T> action)
        {
            AddIdleDelegateIgnoreReturn(action, _pendingDelegates.Count);
        }

        public static void AddIdleDelegate(MethodInvoker action, int priority)
        {
            AddIdleDelegateInternal(new MethodInvoker(delegate()
            {
                action.Invoke();
            }), priority);
        }

        public static void AddIdleDelegateIgnoreReturn<T>(e2IdleDelegate<T> action, int priority)
        {
            AddIdleDelegateInternal(new MethodInvoker(delegate()
                {
                    action.Invoke();
                }), priority);            
        }

        private static void AddIdleDelegateInternal(MethodInvoker action, int priority)
        {
            lock (_pendingDelegates)
            {
                _pendingDelegates.Add(new e2IdleEntry
                    {
                        Delegate = action,
                        QueueStackTrace = new StackTrace(true),
                        Priority = priority
                    });
            }
        }

        public static void UseWaitCursorHere()
        {
            Cursor.Current = Cursors.WaitCursor;
            e2Forms.AddIdleDelegateBlock(delegate() { Cursor.Current = Cursors.Default; });
        }

        private static ToolTip _globalTip = new ToolTip();

        public static void GlobalTip(ToolTipIcon icon, string title, string message)
        {
            if (Form.ActiveForm != null)
            {
                _globalTip.ToolTipIcon = icon;
                _globalTip.ToolTipTitle = title;
                _globalTip.Show(message, Form.ActiveForm, 0, Form.ActiveForm.Height);
            }            
        }
        
        private static void DisplayWaitingForms()
        {
            foreach (e2WindowForm form in _delayedDisplayForms)
            {
                for (double op = 0; op < 1; op += 0.05)
                {
                    form.Opacity = op;
                    Thread.Sleep(5);
                }

                form.Opacity = 1;
            }

            _delayedDisplayForms.Clear();            
        }
        
        private static void CheckActiveForm()
        {
            Form form = Form.ActiveForm;
            if (form != null)
            {
                if (!form.GetType().IsSubclassOf(typeof(e2WindowForm)))
                {
                    form.Text = "e2: use e2DialogForm/e2WindowForm as base classes for forms.";
                }
                else
                {
                    CheckControls(form, form);
                }
            }
        }

        private static void CheckControls(Form form, Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (!(child is Ie2Control))
                {
                    form.Text = "e2: " + child.Name + " is not allowed control.";
                }
                
                CheckControls(form, child);                
            }
        }

        public static MulticastDelegate GetControlEvent(Control control, string eventName)
        {
            try
            {
                object eventKey = typeof(Control).GetField("Event" + eventName, BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                EventHandlerList events = (EventHandlerList)control.GetType().GetProperty("Events", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control, null);

                return events[eventKey] as MulticastDelegate;
            }
            catch
            {
                return null;
            }
        }
    }

    public delegate T e2IdleDelegate<T>();

    public class e2IdleEntry : IComparable<e2IdleEntry>
    {
        public Delegate Delegate;
        public StackTrace QueueStackTrace;
        public int Priority = 0;

        #region IComparable<e2IdleDelegate> Members

        public int CompareTo(e2IdleEntry other)
        {
            return this.Priority.CompareTo(other.Priority);
        }

        #endregion

        public override string ToString()
        {
            return String.Format("{0}.{1} ({2})",
                this.Delegate.Method.DeclaringType.Name,
                this.Delegate.Method.Name,
                this.Priority);
        }
    }
}

