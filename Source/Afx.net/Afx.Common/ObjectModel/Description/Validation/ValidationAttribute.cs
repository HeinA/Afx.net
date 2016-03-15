using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Afx.ObjectModel.Description.Validation
{
  [AttributeUsage(AttributeTargets.Property)]
  public abstract class ValidationAttribute : Attribute
  {
    protected ValidationAttribute(string message, bool concurrent)
    {
      Message = message;
      Concurrent = concurrent;
    }

    protected ValidationAttribute(string message)
    {
      Message = message;
    }

    #region string Message

    string mMessage;
    public string Message
    {
      get { return mMessage; }
      private set { mMessage = value; }
    }

    #endregion

    #region bool Concurrent

    bool mConcurrent = false;
    public bool Concurrent
    {
      get { return mConcurrent; }
      private set { mConcurrent = value; }
    }

    #endregion

    public abstract bool Validate(object target, PropertyInfo pi, CancellationToken cancellationToken);
  }
}
