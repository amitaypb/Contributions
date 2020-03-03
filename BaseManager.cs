using BusinessObjects.DTOs.General;
using BusinessObjects.Enums.Admin;
using BusinessObjects.Enums.General;
using Framework.BusinessLogic.Util;
using Framework.Repositories.Base;
using Framework.Repositories.General;
using Framework.Utilities.Util;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Framework.BusinessLogic.Base
{
    /// <summary>
    /// Base class of all Managers. Both Business Objects and not Business Objects (Objects not stored in the Database.)
    /// </summary>
    public abstract class BaseManager
    {
        #region Constructor

        public BaseManager(string objectName)
        {
            ObjectName = objectName;
        }

        public BaseManager(string objectName, object viewModel)
        {
            ObjectName = objectName;
            ViewModel = viewModel;
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Name of the Object using the Repository. This is used in QueryMonitor to know
        /// which object executed the Query.
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// ViewModel using the Manager.
        /// This is used to allow the Managers to set the View as Busy.
        /// </summary>
        public object ViewModel { get; set; }

        private ValidatorUtil _validatorUtil;
        protected ValidatorUtil ValidatorUtil
        {
            get { return _validatorUtil ?? (_validatorUtil = new ValidatorUtil()); }
        }

        private ManagerUtil _managerUtil;
        protected ManagerUtil ManagerUtil
        {
            get { return _managerUtil ?? (_managerUtil = new ManagerUtil(ObjectName)); }
        }

        private BaseRepository _repository;

        public virtual BaseRepository Repository
        {
            get { return _repository ?? (_repository = new BaseRepository(ObjectName, ViewModel)); }
        }

        public DatabaseVendorEnum DatabaseProvider
        {
            get { return Repository.DatabaseProvider; }
            set { Repository.DatabaseProvider = value; }
        }

        public DatabaseLoginDTO DatabaseLoginDTO
        {
            get { return Repository == null ? null : Repository.DatabaseLoginDTO; }
            set
            {
                if (Repository == null)
                {
                    return;
                }

                Repository.DatabaseLoginDTO = value;
            }
        }

        public string DatabaseConnectionString
        {
            get { return Repository == null ? string.Empty : Repository.DatabaseConnectionString; }
            set
            {
                if (Repository == null)
                {
                    return;
                }

                Repository.DatabaseConnectionString = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
		/// Executes a Stored Procedure.
		/// </summary>
		/// <param name="storedProcedureName">Name of the Stored Procedure.</param>
		/// <param name="schemaName">Name of the Schema where the Stored Procedure 
		/// exists in the Database.</param>
		/// <param name="parameters">Parameters for the Stored Procedure.</param>
		/// <returns>Task</returns>
		public virtual async Task ExecuteStoredProcedure(string storedProcedureName)
        {
            await Repository.ExecuteStoredProcedure(storedProcedureName);
        }

        /// <summary>
		/// Sets the Database Util used to Connect to the Database.
		/// </summary>
		/// <param name="databaseRepository">Database Util.</param>
		/// <param name="onlyThisInstance">TRUE: The Database Util will be used only for
		/// this instance.
		/// FALSE: The Database Util will be used in every instance.</param>
		public virtual void SetDatabaseRepository(DatabaseRepository databaseRepository, bool onlyThisInstance = false)
        {
            Repository.SetDatabaseRepository(databaseRepository, onlyThisInstance);
        }

        /// <summary>
		/// Sets the Database Util used to Connect to the Database.
		/// </summary>
		/// <param name="databaseUtilFactory">Database Util Factory.</param>
		/// <param name="onlyThisInstance">TRUE: The Database Util will be used only for
		/// this instance.
		/// FALSE: The Database Util will be used in every instance.</param>
		public virtual void SetDatabaseRepository(DatabaseVendorEnum databaseVendorEnum, DatabaseLoginDTO databaseLoginDTO, bool onlyThisInstance = false)
        {
            Repository.SetDatabaseRepository(ObjectName, databaseVendorEnum, databaseLoginDTO, onlyThisInstance);
        }

        /// <summary>
        /// Sets the Database Util used to Connect to the Database.
        /// </summary>
        /// <param name="connectionString">Connection String.</param>
        /// <param name="onlyThisInstance">TRUE: The Database Util will be used only for
        /// this instance.
        /// FALSE: The Database Util will be used in every instance.</param>
        public virtual void SetDatabaseRepository(string connectionString, bool onlyThisInstance)
        {
            Repository.SetDatabaseRepository(connectionString, onlyThisInstance);
        }

        /// <summary>
        /// Gets the database Provider Enum given the string.
        /// </summary>
        /// <param name="objectName">Name of the Object that is going to execute Queries.</param>
        /// <param name="databaseProvider">Database provider.</param>
        /// <param name="serverName">Server Name</param>
        /// <param name="databaseName">Database Name</param>
        /// <param name="instanceName">Instance Name</param>
        /// <param name="userName">User name</param>
        /// <param name="password">Password</param>
        /// <param name="port">Port</param>
        public virtual void SetDatabaseRepository(string objectName, DatabaseVendorEnum databaseProvider, string serverName,
            string databaseName, string instanceName, string userName, string password, int port)
        {
            Repository.SetDatabaseRepository(objectName, databaseProvider, serverName, databaseName, instanceName, userName, password, port);
        }

        /// <summary>
        /// Gets the database Provider Enum given the string.
        /// </summary>
        /// <param name="databaseProviderId">Id of the Database Provider.</param>
        public virtual void SetDatabaseRepository(string objectName, long databaseProviderId, string connectionString, bool ignoreSpaces = false)
        {
            Repository.SetDatabaseRepository(objectName, databaseProviderId, connectionString, ignoreSpaces);
        }

        public virtual async Task<DateTime> GetDatabaseTime()
        {
            return await Repository.GetDatabaseTime();
        }

        public virtual void SetDatabaseRepositoryFromSession()
        {
            Repository.SetDatabaseRepositoryFromSession();
        }

        /// <summary>
        /// Delays the execution in the specified Miliseconds.
        /// This Method can be used to convert a Synchronous Method into an Asynchronous Method.
        /// </summary>
        /// <param name="miliseconds">Miliseconds to Delay the execution.</param>
        /// <returns>Task.</returns>
        public virtual async Task DelayTask(int miliseconds = 1)
        {
            await ManagerUtil.DelayTask(miliseconds);
        }

        /// <summary>
		/// Gets the System Text with the specified Label for the specified Object Name.
		/// </summary>
		/// <param name="label">Label of the System Text.</param>
		/// <param name="defaultValue">Default Value if no Text is Found.</param>
		/// <param name="objectName">Name of the object that the Text belongs to.</param>
		/// <param name="formatString">String to format the text.</param>
		/// <returns>System Text with the specified Label for the specified Object Name.</returns>
		public virtual async Task<string> GetText<T>(Expression<Func<T>> propertyExpression,
            string defaultValue = "", string objectName = "", object formatString = null, object formatString2 = null)
        {
            var propertyName = PropertyUtil.ExtractPropertyName(propertyExpression);

            return await GetText(propertyName, defaultValue, objectName, formatString, formatString2);
        }

        /// <summary>
        /// Gets the System Text with the specified Label for the specified Object Name.
        /// </summary>
        /// <param name="label">Label of the System Text.</param>
        /// <param name="defaultValue">Default Value if no Text is Found.</param>
        /// <param name="objectName">Name of the object that the Text belongs to.</param>
        /// <param name="formatString">String to format the text.</param>
        /// <returns>System Text with the specified Label for the specified Object Name.</returns>
        public virtual async Task<string> GetText(string label, string defaultValue = "",
            string objectName = "", object formatString = null, object formatString2 = null)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                objectName = GetType().FullName;
            }

            var text = await ManagerUtil.GetText(label, defaultValue, objectName, formatString, formatString2);

            //If no Text was found for the current object, try searching the Base Object.
            if (string.IsNullOrEmpty(text))
            {
                objectName = GetType().BaseType.FullName;

                text = await ManagerUtil.GetText(label, defaultValue, objectName, formatString, formatString2);
            }

            return text;
        }

        /// <summary>
		/// Gets the System Text with the specified Label for the specified Object Name.
        /// Use this Method only in specific situations, where you are not able to execute an async MEthod.
        /// For instance, in the Closing Event of a Window. Otherwise, use the async GetText Method.
		/// </summary>
		/// <param name="label">Label of the System Text.</param>
		/// <param name="defaultValue">Default Value if no Text is Found.</param>
		/// <param name="objectName">Name of the object that the Text belongs to.</param>
		/// <param name="formatString">String to format the text.</param>
		/// <returns>System Text with the specified Label for the specified Object Name.</returns>
		public virtual string GetTextSync(string label, string defaultValue = "",
            string objectName = "", object formatString = null, object formatString2 = null)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                objectName = GetType().FullName;
            }

            return ManagerUtil.GetTextSync(label, defaultValue, objectName, formatString, formatString2);
        }

        /// <summary>
        /// Gets the List of Databases associated to the given serverName
        /// </summary>
        /// <param name="serverName">Name of the server</param>
        /// <returns>List of Databases associated to the given serverName</returns>
        public virtual async Task<List<DatabaseDTO>> GetDatabases(string serverName)
        {
            return await Repository.GetDatabases(serverName);
        }

        /// <summary>
        /// Gets the List of Databases associated to the given serverName
        /// </summary>
        /// <param name="serverName">Name of the server</param>
        /// <returns>List of Databases associated to the given serverName</returns>
        public virtual List<DatabaseDTO> GetDatabasesSync(string serverName)
        {
            return Repository.GetDatabasesSync(serverName);
        }

        public virtual async Task<string> TestDatabaseConnection()
        {
            var result = await Repository.TestDatabaseConnection();
            return result;
        }

        public virtual string ConnectionString()
        {
            return Repository.ConnectionString();
        }

        /// <summary>
        /// Get the List of Tables name in the given dataBase and server
        /// </summary>
        /// <param name="excludeCatalogTables">For PostgreSql: Indicates if the tables from the pg_catalog 
        /// and information_schema schemas will be excluded.</param>
        /// <param name="fullLoaded">Indicates if we want to return all of the Tables with Columns.
        /// (This is slower, because the Columns needs to be queried into the Database foreach Table.)</param>
        /// <returns>List of Tables name in the given dataBase and server</returns>
        public virtual async Task<List<TableDTO>> GetDatabaseTables(bool excludeCatalogTables = true, bool fullLoaded = false)
        {
            var tables = await Repository.GetDatabaseTables(excludeCatalogTables);

            if(fullLoaded)
            {
                //Get all of the Columns of every table.
                foreach (var table in tables)
                {
                    table.Columns = await GetColumnsFromTable(table.Name);
                }
            }

            return tables;
        }

        /// <summary>
        /// Get the column names and types of the specified table, in the specified
        /// database in the specified server
        /// </summary>
        /// <param name="tableName">Table name without schema name.</param>
        /// <returns>List of column names and types of the specified table, in the specified
        /// database in the specified server</returns>
        public virtual async Task<List<ColumnDTO>> GetColumnsFromTable(string tableName)
        {
            return await Repository.GetColumnsFromTable(tableName);
        }

        /// <summary>
        /// Get the List of all of the Stored Procedures in the Database.
        /// </summary>
        /// <param name="getParameters">Indicates if the Parameters of each Stored Procedure will be returned.
        /// False by Default, because it requires extra calculation and time.</param>
        /// <returns>List of all of the Stored Procedures in the Database.</returns>
        public virtual async Task<List<StoredProcedureDTO>> GetAllStoredProcedures(bool getParameters = false, bool getTables = false)
        {
            var storedProcedureDTOs = await Repository.GetAllStoredProcedures(getParameters);

            if (getTables)
            {
                var message = await GetText("SearchingStoredProceduresTables", "", typeof(BaseManager).FullName);
                SetViewIsBusy(message);
                await DelayTask();

                var sqlParser = new SQLParser();

                foreach (var storedProcedureDTO in storedProcedureDTOs)
                {
                    try
                    {
                        //Get all of the Statements of the Stored Procedure.
                        var statements = sqlParser.GetStatements(storedProcedureDTO.Content, PermissionTypeEnum.AllCRUD);

                        //Check all of the Statements of the Stored Procedure.
                        foreach (var statement in statements)
                        {
                            var tables = sqlParser.GetTablesInStatement(statement, storedProcedureDTO.Name);

                            var sqlStatement = new SQLStatementDTO
                            {
                                Content = statement,
                                Tables = tables
                            };

                            storedProcedureDTO.SQLStatements.Add(sqlStatement);
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }

            return storedProcedureDTOs;
        }

        /// <summary>
        /// Gets all of the Schemas in the current Database.
        /// </summary>
        /// <param name="ignoreNativeSchemas">Indicates if the Native Schemas of the Database will be ignored.
        /// For instance: For PostgreSQL, ignore the schemas starting with the pg_ prefix.</param>
        /// <returns>List of Schemas in the current Database.</returns>
        public async Task<List<SchemaDTO>> GetDatabaseSchemas(bool ignoreNativeSchemas = true)
        {
            return await Repository.GetDatabaseSchemas(ignoreNativeSchemas);
        }

        /// <summary>
        /// Gets the List of User Defined Types in the Database.
        /// </summary>
        /// <returns>List of User Defined Types in the Database.</returns>
        public async Task<List<string>> GetUserDefinedTypesNames()
        {
            return await Repository.GetUserDefinedTypesNames();
        }

        /// <summary>
        /// Gets the List of the User Defined Types in the Database.
        /// </summary>
        /// <returns>List of User Defined Types in the Database.</returns>
        public async Task<List<UserDefinedTypeDTO>> GetUserDefinedTypesDTOs()
        {
            return await Repository.GetUserDefinedTypesDTOs();
        }

        /// <summary>
        /// Sets the View as Busy.
        /// </summary>
        /// <param name="message">Busy message.</param>
        /// <param name="showProgressBar">True: The Circle Balls Busy Indicator is displayed.
        /// False: The ProgressBar Indicator is displayed. For an example of usage, see the FileCopierView.</param>
        public void SetViewIsBusy(string message, bool showProgressBar = false)
        {
            Repository.SetViewIsBusy(message, showProgressBar);
        }

        public async Task SetProgressBarValue(int total, int part, string message = "")
        {
            await Repository.SetProgressBarValue(total, part, message);
        }

        /// <summary>
        /// Executes the specified script file in the database.
        /// </summary>
        /// <param name="scriptFilePath">Path of the script file.</param>
        /// <param name="isFile">
        /// TRUE: The parameter scriptFilePath is a Path to a File.
        /// FALSE: The parameter scriptFilePath is a Script as a String.</param>
        /// <returns></returns>
        public virtual async Task<object> ExecuteScript(string scriptFilePath, bool isFile = true)
        {
            return await Repository.ExecuteScript(scriptFilePath, isFile);
        }

        public string SchemaName()
        {
            return Repository.SchemaName();
        }

        #endregion Methods
    }
}
