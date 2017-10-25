﻿using Server.Interfaces;
using Server.Interfaces.ClientRegistration;

namespace Server.Modules.ClientRegistrationModule
{
    public class CheckForAlreadyExistsUserInDB : ICheckForAlreadyExistsUser
    {
        private readonly IDataBase DataBase;

        public CheckForAlreadyExistsUserInDB(IDataBase dataBase)
        {
            DataBase = dataBase;
        }

        private string[] QueryResult;

        public void GetData(string userName, string userEmail)
        {
            DataBase.bind(new string[] { "Login", userName, "Email", userEmail });
            DataBase.manySelect("SELECT login, email, register_id FROM users WHERE login = @Login OR email = @Email");
            QueryResult = DataBase.tableToRow();
        }

        public bool isLoginExists()
        {
            string query = null;
            if (QueryNotNull(QueryResult))
                query = QueryResult[0];
            if (query != null)
                return true;
            return false;
        }

        public bool isEmailExists()
        {
            string query = null;
            if (QueryNotNull(QueryResult))
                query = QueryResult[1];
            if (query != null)
                return true;
            return false;
        }

        public bool isUserAlreadyRegister()
        {
            string query = null;
            if (QueryNotNull(QueryResult))
                query = QueryResult[2];
            if (query != null)
                return true;
            return false;
        }

        private bool QueryNotNull(string[] QueryResult)
        {
            if (QueryResult != null)
                return true;
            return false;
        }

    }
}
