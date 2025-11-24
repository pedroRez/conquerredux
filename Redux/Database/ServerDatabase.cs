using Redux.Database.Repositories;

namespace Redux.Database
{
    public class ServerDatabase
    {
        public static ConquerDataContext Context { get; private set; }

        public static bool InitializeSql()
        {
            Context = new ConquerDataContext();
            NHibernateHelper.BuildSessionFactory();

            // Initialize the item generator seed before any new items are created
            Context.Items.PopulateItemGenerator();

            Context.Accounts.ResetLoginTokens();
            Context.Characters.ResetOnlineCharacters();
            return true;
        }
    }
}
