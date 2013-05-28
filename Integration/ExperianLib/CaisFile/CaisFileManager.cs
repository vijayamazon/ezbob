namespace ExperianLib.CaisFile
{
    public class CaisFileManager
    {
        private static BusinessCaisFileData _business;
        private static CaisFileData _consumer;

        public static BusinessCaisFileData GetBusinessCaisFileData()
        {
            return _business ?? (_business = new BusinessCaisFileData());
        }

        public static void RemoveBusinessCaisFileData()
        {
            _business = null;
        }

        public static CaisFileData GetCaisFileData()
        {
            return _consumer ?? (_consumer = new CaisFileData());
        }

        public static void RemoveCaisFileData()
        {
            _consumer = null;
        }
    }
}