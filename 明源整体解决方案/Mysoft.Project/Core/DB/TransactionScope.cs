using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;
namespace Mysoft.Project.Core
{
    // Transaction object helps maintain transaction depth counts
    public class Transaction : IDisposable
    {
      
        static Action<Database> _CompleteTransaction;
        static Action<Database> _AbortTransaction;
        static Transaction() {        
            _CompleteTransaction = GetDynamicDelegate("CompleteTransaction");
            _AbortTransaction = GetDynamicDelegate("AbortTransaction");
           
        }
        static Action<Database> GetDynamicDelegate(string methodName) {
            // Create an array that specifies the types of the parameters
            // of the dynamic method. This method has a string parameter
            // and an int parameter.
            Type[] dbArgs = { typeof(Database) };
            DynamicMethod dm = new DynamicMethod("Database_" + methodName, null, dbArgs, typeof(Transaction),true);         
            MethodInfo mi = typeof(Database).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            // Get an ILGenerator and emit a body for the dynamic method.
            ILGenerator il = dm.GetILGenerator();
            // Load the first argument, which is a string, onto the stack.
            il.Emit(OpCodes.Ldarg_0);
            // Call the overload of Console.WriteLine that prints a string.
            il.Emit(OpCodes.Callvirt, mi);           
            il.Emit(OpCodes.Ret);
            // Create a delegate that represents the dynamic method. This
            // action completes the method, and any further attempts to
            // change the method will cause an exception.          
            return (Action<Database>)dm.CreateDelegate(typeof(Action<Database>));
        }
        public Transaction(Database db)
        {
            _db = db;          
        }

        public virtual void Complete()
        {
            if (_db != null)
            {
                _CompleteTransaction(_db);

                _db = null;
            }
        }

        public void Dispose()
        {
            if (_db != null)
                _AbortTransaction(_db);
        }

        Database _db;
    }
}
