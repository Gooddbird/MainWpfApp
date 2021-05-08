using MainWpfApp.ViewModels;
using SQLite;
using System;

namespace MainWpfApp.Util {
    public class DbConnection : SQLiteConnection {

        public TableQuery<BoltModel> Bolts { get { return this.Table<BoltModel>(); } }
        public TableQuery<BoltLogModel> BoltLogs { get { return this.Table<BoltLogModel>(); } }

        public DbConnection(string path) : base(path) {
            CreateTable<BoltModel>();
            CreateTable<BoltLogModel>();
        }
        public void CreateInitTable() {
            try
            {
                foreach (BoltModel tmp in Bolts.ToList())
                {
                    string zeroTableName = "t_zero_" + tmp.Bolt_id;
                    // 零应力表
                    string sql1 = "CREATE TABLE IF NOT EXISTS " + zeroTableName +
                        "(Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "Bolt_id VARCHAR NOT NULL, " +
                        "Position INT NOT NULL, " +
                        "Data FLOAT NOT NULL);";
                    string logTableName = "t_log_" + tmp.Bolt_id;
                    // 测量结果记录表
                    string sql2 = "CREATE TABLE IF NOT EXISTS " + logTableName +
                        "(Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "Bolt_id VARCHAR NOT NULL, " +
                        "axialForce FLOAT NOT NULL, " +
                        "timeDelay FLOAT NOT NULL, " +
                        "MaxXcorr FLOAT NOT NULL, " +
                        "CurrentTime VARCHAR NOT NULL);";
                    Execute(sql1);
                    Execute(sql2);
                }
                Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine("exception:", e.ToString());
                Rollback();
            }
        }
    }
}
