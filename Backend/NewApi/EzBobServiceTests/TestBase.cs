using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobServiceTests {
    using System.Globalization;
    using System.Reflection;
    using System.Web;
    using NServiceBus;
    using NServiceBus.Settings;
    using StructureMap;

    public abstract class TestBase {

        private static readonly string Alphabet = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string PasswordAlphaBet = "abcdefghijklmnopqrstuvwxyzABCDFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$;+";
        private static readonly string[] TLDs;
        protected static readonly Random Random = new Random();

        static TestBase() {
            string tlds = EzBobServiceTests.Properties.Resources.tlds_alpha_by_domain;
            TLDs = tlds.Split(' ', '\n');
        }

        /// <summary>
        /// Initializes the container, and scans assembly of provided type for registry
        /// </summary>
        /// <param name="scanAssemblyOfType">Type of the scan assembly of.</param>
        /// <returns></returns>
        protected IContainer InitContainer(Type scanAssemblyOfType) {
           
            Container container = new Container();
            container.Configure(c => c.Scan(scanner => {
                scanner.AssemblyContainingType(scanAssemblyOfType);
                scanner.LookForRegistries();
            }));

            return container;
        }

        /// <summary>
        /// Creates the NSB test container from structure map container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns></returns>
        protected NServiceBus.ObjectBuilder.Common.IContainer CreateNsbTestContainer(StructureMap.IContainer container) {
            StructureMapBuilder builder = new StructureMapBuilder();
            SettingsHolder settings = new SettingsHolder();
            settings.Set("ExistingContainer", container);
            return builder.CreateContainer(settings);
        }

        /// <summary>
        /// Gets the random password.
        /// </summary>
        /// <param name="passwordlLength">Length of the passwordl.</param>
        /// <returns></returns>
        public string GetRandomPassword(uint passwordlLength = 8) {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < passwordlLength; ++i) {
                int idx = Random.Next(0, PasswordAlphaBet.Length - 1);
                builder.Append(PasswordAlphaBet[idx]);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets the rundom integer.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns></returns>
        public int GetRundomInteger(int minValue, int maxValue) {
            return Random.Next(minValue, maxValue);
        }

        /// <summary>
        /// Gets the random email address.
        /// </summary>
        /// <returns></returns>
        public string GetRandomEmailAddress() {
            StringBuilder builder = new StringBuilder();
            this.GenerateName(7, 3, builder);
            builder.Append("@");
            this.GenerateName(5, 0, builder);
            builder.Append(".");

            int tldIdx = Random.Next(0, TLDs.Length - 1);
            builder.Append(TLDs[tldIdx].Trim());
            return builder.ToString();
        }

        /// <summary>
        /// Generates the name.
        /// </summary>
        /// <param name="numberOfChars">The number of chars.</param>
        /// <param name="numberOfDigits">The number of digits.</param>
        /// <param name="builder">The builder.</param>
        private void GenerateName(uint numberOfChars, uint numberOfDigits, StringBuilder builder) {
            int alphaBetLength = Alphabet.Length;

            for (int i = 0; i < Math.Max(1, numberOfChars); ++i) {
                int idx = Random.Next(0, alphaBetLength - 1);
                builder.Append(Alphabet[idx]);
            }

            for (int i = 0; i < numberOfDigits; ++i) {
                builder.Append(Random.Next(0, 9));
            }
        }

        /// <summary>
        /// Checks whether two object have equal properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o1">The o1.</param>
        /// <param name="o2">The o2.</param>
        /// <returns></returns>
        protected bool AreEqualByWritableReadableProperties<T>(T o1, T o2) where T : class {
            PropertyInfo[] propertyInfos = ObtainAllTypeProperties<T>();
            foreach (PropertyInfo propertyInfo in propertyInfos.Where(prop => prop.CanRead && prop.CanWrite)) {
                var prop1 = propertyInfo.GetValue(o1);
                var prop2 = propertyInfo.GetValue(o2);
                if ((prop1 == null && prop2 != null) || (prop1 != null && prop2 == null)) {
                    if (propertyInfo.PropertyType == typeof(string)) {
                        if ("N/A".Equals(prop1) || "N/A".Equals(prop2)) {
                            continue;
                        }
                    }
                    return false;
                }

                if (prop1 == null && prop2 == null)
                    continue;

                if (propertyInfo.PropertyType == typeof(DateTime)) {
                    if (prop1.ToString().Equals(prop2.ToString()))
                    {
                        continue;
                    }

                    return false;
                }

                if (!prop1.Equals(prop2)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Generates the name.
        /// </summary>
        /// <param name="nameLength">Length of the name.</param>
        /// <returns></returns>
        protected string GenerateName(int nameLength = 5) {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < nameLength; ++i)
            {
                int idx = Random.Next(0, Alphabet.Length - 1);
                builder.Append(Alphabet[idx]);
            }

            return builder.ToString();
        }


        /// <summary>
        /// Generates the visit times string.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        protected string GenerateVisitTimesString(int count = 3) {
            DateTime now = DateTime.UtcNow;
            String visitedTimes = Enumerable.Range(1, Math.Max(count, 2))
                .Select(i => now.AddDays(-i)
                    .ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture))
                .Aggregate("", (s, s1) => s + s1 + ";");

            return HttpUtility.UrlEncode(visitedTimes);
        }

        /// <summary>
        /// Obtains all type properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private PropertyInfo[] ObtainAllTypeProperties<T>() {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

    }
}
