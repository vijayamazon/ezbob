namespace EzBobPersistence.QueryGenerators {
    using System;
    using System.Data.SqlClient;
    using System.Linq.Expressions;
    using System.Reflection;

    public abstract class CommandGeneratorBase {
        /// <summary>
        /// Helps with fluent interface<br></br> 
        /// provides indirect access to GenerateCommand() method (which is not public)
        /// </summary>
        private class Generator : ISqlCommandGenerator {
            private CommandGeneratorBase generator;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Object"/> class.
            /// </summary>
            public Generator(CommandGeneratorBase generator) {
                this.generator = generator;
            }

            public SqlCommand GenerateCommand() {
                return this.generator.GenerateCommand();
            }
        }

        /// <summary>
        /// Verifies this instance.
        /// </summary>
        public abstract ISqlCommandGenerator Verify();

        /// <summary>
        /// Gets the generator.
        /// </summary>
        /// <returns></returns>
        protected ISqlCommandGenerator GetGenerator() {
            return new Generator(this);
        }

        /// <summary>
        /// Generates the command.
        /// </summary>
        /// <returns></returns>
        protected abstract SqlCommand GenerateCommand();

        /// <summary>
        /// Extracts the name of the member.
        /// </summary>
        /// <param name="propertyAccessExpression">The property access expression.</param>
        /// <returns>property name. (for expression o=>o.Age will return 'Age')</returns>
        /// <exception cref="System.ArgumentException">expected lambda like o => o.Age</exception>
        protected string ExtractMemberName<T>(Expression<Func<T, object>> propertyAccessExpression) where T : class {
            MemberExpression memberExpression = propertyAccessExpression.Body as MemberExpression;

            if (memberExpression == null) {
                //it could be a Convert expression, for example, if property type is struct
                UnaryExpression u = propertyAccessExpression.Body as UnaryExpression;
                if (u != null) {
                    memberExpression = u.Operand as MemberExpression;
                }
            }

            if (memberExpression == null) {
                throw new ArgumentException("expected lambda like o => o.Age");
            }

            return memberExpression.Member.Name;
        }

        /// <summary>
        /// Obtains all properties of specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected PropertyInfo[] ObtainAllTypeProperties<T>() {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
