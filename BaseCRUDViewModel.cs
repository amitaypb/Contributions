using BusinessObjects.BusinessObjects.Base;
using BusinessObjects.DTOs.Parameters;
using BusinessObjects.Interfaces;
using BusinessObjects.Util;
using Base.Constants;
using Framework.Utilities.Util;
using GalaSoft.MvvmLight.Command;
using Framework.BusinessLogic.Base;
using Framework.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Framework.Views.General;
using Framework.UserControls.General;
using System.Windows;
using Framework.Views.Base;
using Framework.BusinessLogic.Util;
using Framework.ViewModels.General;
using Base.Attributes;
using BusinessObjects.Enums.General;
using BusinessObjects.Enums.Admin;

namespace Framework.ViewModels.Base
{
    /// <summary>
    /// ViewModel for the Views or UserControls with Texts in the Database.
    /// </summary>
    public class BaseCRUDViewModel<T> : BaseViewModel
        where T : BaseBusinessObject, new()
    {
        #region Constructor

        public BaseCRUDViewModel()
        {
            SearchBaseTypeTexts = true;
            SearchCommand = new RelayCommand(SearchCommandAction);
            CancelCommand = new RelayCommand(CancelCommandAction);
            NewBusinessObjectCommand = new RelayCommand(NewBusinessObjectAction);
            DeleteBusinessObjectCommand = new RelayCommand(DeleteBusinessObjectCommandAction);
            GoToFirstPageCommand = new RelayCommand(GoToFirstPageCommandAction);
            GoToPreviousPageCommand = new RelayCommand(GoToPreviousPageCommandAction);
            GoToNextPageCommand = new RelayCommand(GoToNextPageCommandAction);
            GoToLastPageCommand = new RelayCommand(GoToLastPageCommandAction);
            RestoreSearchDefaultValuesCommand = new RelayCommand(RestoreSearchDefaultValuesAction);
            AboutCommand = new RelayCommand(AboutCommandAction);
            ImportExcelCommand = new RelayCommand(ImportExcelCommandAction);
			OpenImportDocumentViewCommand = new RelayCommand(OpenImportDocumentView);
			EditBusinessObjectCommand = new RelayCommand(EditBusinessObjectAction);
            MagnifyingGlassCommand = new RelayCommand(MagnifyingGlassCommandAction);
            CloneSelectedBusinessObjectsCommand = new RelayCommand(CloneSelectedBusinessObjectsAction);

            SelectAllBusinessObjectsCommand = new RelayCommand<object>(SelectAllBusinessObjectsAction);
            CheckedBusinessObjectCommand = new RelayCommand<object>(CheckedBusinessObjectAction);
            UnCheckedBusinessObjectCommand = new RelayCommand<object>(UnCheckedBusinessObjectAction);

            ExcludedAuditUsersOnSearch = true;

            RestoreSearchDefaultValues();
        }

        #endregion Constructor

        #region Properties

        public virtual BaseBusinessObjectManager<T> Manager { get; }

        private List<T> _businessObjects;
        /// <summary>
        /// List of Business Objects.
        /// </summary>
        public virtual List<T> BusinessObjects
        {
            get { return _businessObjects; }
            set
            {
                _businessObjects = value;
                NotifyOfPropertyChange(() => BusinessObjects);
                NotifyOfPropertyChange(() => BusinessObjectsCount);
            }
        }

        public virtual long BusinessObjectsCount
        {
            get { return BusinessObjects == null ? 0 : BusinessObjects.Count; }
        }

        public T _selectedBusinessObject;
        /// <summary>
        /// Selected Business Object in the Search DataGrid.
        /// </summary>
        public T SelectedBusinessObject
        {
            get { return _selectedBusinessObject; }
            set
            {
                BeforeSelectBusinessObject();

                IsLoadingProperties = true;
                _selectedBusinessObject = value;
                NotifyOfPropertyChange(() => SelectedBusinessObject);
                NotifyOfPropertyChange(() => SelectedBusinessObjectNotNull);

                AfterSelectBusinessObjectAction();

                //When we select a Business Object, we hide the New Business Object FlyOut, to be able to display
                //the Selected Business Object Properties.
                ShowNewBusinessObjectFlyOut = false;
            }
        }

        private List<PropertyInfo> _businessObjectProperties;

        protected List<PropertyInfo> BusinessObjectProperties
        {
            get
            {
                if(_businessObjectProperties == null)
                {
                    _businessObjectProperties = ReflectionUtil.GetPropertiesInfo<T>();
                }

                return _businessObjectProperties;
            }
        }

        private PropertyInfo _businessObjectNameProperty;
        protected PropertyInfo BusinessObjectNameProperty
        {
            get
            {
                if(_businessObjectNameProperty == null)
                {
                    _businessObjectNameProperty = BusinessObjectProperties.FirstOrDefault(x => x.Name == BaseConstants.Name);
                }

                return _businessObjectNameProperty;
            }
        }

        public string SelectedBusinessObjectName
        {
            get
            {
                if(SelectedBusinessObject == null || BusinessObjectNameProperty == null)
                {
                    return string.Empty;
                }

                var name = BusinessObjectNameProperty.GetValue(SelectedBusinessObject);

                return name == null ? string.Empty : name.ToString();
            }
            set
            {
                if (SelectedBusinessObject == null || BusinessObjectNameProperty == null)
                {
                    return;
                }

                BusinessObjectNameProperty.SetValue(SelectedBusinessObject, value);
            }
        }

        public bool SelectedBusinessObjectNotNull
        {
            get { return SelectedBusinessObject != null; }
        }

        private bool _showNewBusinessObjectFlyOut;
        public bool ShowNewBusinessObjectFlyOut
        {
            get { return _showNewBusinessObjectFlyOut; }
            set
            {
                if(_showNewBusinessObjectFlyOut == value)
                {
                    return;
                }

                if(!value)
                {
                    NewBusinessObject = null;
                }

                _showNewBusinessObjectFlyOut = value;
                NotifyOfPropertyChange(() => ShowNewBusinessObjectFlyOut);
            }
        }

        private T _newBusinessObject;

        /// <summary>
        /// Business Object created in the New Model Command.
        /// </summary>
        public T NewBusinessObject
        {
            get { return _newBusinessObject; }
            set
            {
                _newBusinessObject = value;
                NotifyOfPropertyChange(() => NewBusinessObject);

                AssignCancelButtonText();
            }
        }

        private string _endPage;
        public string EndPage
        {
            get { return _endPage; }
            set
            {
                _endPage = value;
                NotifyOfPropertyChange(() => EndPage);
            }
        }

        private long _businessObjectsTotalCount;

        /// <summary>
        /// Count of All of the Business Objects in the Database that matches the Search.
        /// This is used to know the Total Pages.
        /// </summary>
        public long BusinessObjectsTotalCount
        {
            get { return _businessObjectsTotalCount; }
            set
            {
                _businessObjectsTotalCount = value;
                NotifyOfPropertyChange(() => BusinessObjectsTotalCount);

                TotalRecordsInDatabaseText = string.Format(TotalRecordsInDatabaseToolTip, value);
            }
        }

        /// <summary>
        /// Indicates if the Selected Business Object will be Fully Loaded.
        /// </summary>
        public bool FullLoadBusinessObject { get; set; }

        private bool _isLoadingProperties;
        /// <summary>
        /// Indicates if the View is Loading Properties of the Selected Business Object, to prevent
        /// loading properties when it is Loading Properties. This is done, because the SelectedBusinessObject
        /// is a Property, and cannot be awaited.
        /// </summary>
        public bool IsLoadingProperties
        {
            get { return _isLoadingProperties; }
            set
            {
                _isLoadingProperties = value;
                NotifyOfPropertyChange(() => IsLoadingProperties);
            }
        }

        /// <summary>
        /// Indicates that the Items in the DataGrid are not Business Objects.
        /// </summary>
        public bool NoItemsInDataGrid { get; set; }

		protected Timer SelectedTimer { get; set; }

		private bool _showEditBusinessObjectTab;

		/// <summary>
		/// Indicates if the Edit Business Object Tab is Visible.
		/// </summary>
		public bool ShowEditBusinessObjectTab
		{
			get { return _showEditBusinessObjectTab; }
			set
			{
				if (_showEditBusinessObjectTab == value)
				{
					return;
				}

				_showEditBusinessObjectTab = value;
				NotifyOfPropertyChange(() => ShowEditBusinessObjectTab);

				AssignCancelButtonText();
			}
		}

        private bool _afterSelectBusinessObject;

		/// <summary>
		/// Indicates if the After Select Business Object process have been completed.
		/// </summary>
		public bool AfterSelectBusinessObject
        {
            get { return _afterSelectBusinessObject; }
            set
            {
                if (_afterSelectBusinessObject == value)
                {
                    return;
                }

                _afterSelectBusinessObject = value;
                NotifyOfPropertyChange(() => AfterSelectBusinessObject);
            }
        }

		private bool _loadBusinessObjectToEdit;

		/// <summary>
		/// Indicates if the After Select Business Object process have been completed
		/// and the Edit Tab is selected.
		/// </summary>
		public bool LoadBusinessObjectToEdit
		{
			get { return _loadBusinessObjectToEdit; }
			set
			{
				if (_loadBusinessObjectToEdit == value)
				{
					return;
				}

				_loadBusinessObjectToEdit = value;
				NotifyOfPropertyChange(() => LoadBusinessObjectToEdit);
			}
		}

        private string _totalRecordsInDatabaseText;

        public string TotalRecordsInDatabaseText
        {
            get { return _totalRecordsInDatabaseText; }
            set
            {
                _totalRecordsInDatabaseText = value;
                NotifyOfPropertyChange(() => TotalRecordsInDatabaseText);
            }
        }

        /// <summary>
        /// Indicates if the Business Objects, will be Created / Edited in PopUps.
        /// </summary>
        protected bool CreateEditBusinessObjectsInPopUps { get; set; }

        /// <summary>
        /// Type of the PopUp View to Create / Edit the Business Object.
        /// </summary>
        protected Type PopUpViewType { get; set; }

        /// <summary>
        /// Name of the Assembly where the PopUpView class is defined.
        /// </summary>
        public string PopUpAssemblyName { get; set; }

        private bool _allBusinessObjectsSelected;
        /// <summary>
        /// Indicates if all the Business Objects are selected.
        /// </summary>
        public bool AllBusinessObjectsSelected
        {
            get { return _allBusinessObjectsSelected; }
            set
            {
                _allBusinessObjectsSelected = value;
                SelectAllBusinessObjectsAction(value);
                NotifyOfPropertyChange(() => AllBusinessObjectsSelected);
            }
        }

        #region Multilanguage

        private string _searchingText;
        public string SearchingText
        {
            get { return _searchingText; }
            set
            {
                _searchingText = value;
                NotifyOfPropertyChange(() => SearchingText);
            }
        }

        private string _newButtonTooltip;
        public string NewButtonTooltip
        {
            get { return _newButtonTooltip; }
            set
            {
                _newButtonTooltip = value;
                NotifyOfPropertyChange(() => NewButtonTooltip);
            }
        }

		private string _editButtonToolTip;
		public string EditButtonToolTip
		{
			get { return _editButtonToolTip; }
			set
			{
				_editButtonToolTip = value;
				NotifyOfPropertyChange(() => EditButtonToolTip);
			}
		}

        private string _viewDetailButtonToolTip;
        public string ViewDetailButtonToolTip
        {
            get { return _viewDetailButtonToolTip; }
            set
            {
                _viewDetailButtonToolTip = value;
                NotifyOfPropertyChange(() => ViewDetailButtonToolTip);
            }
        }

        private string _deleteButtonTooltip;
        public string DeleteButtonTooltip
        {
            get { return _deleteButtonTooltip; }
            set
            {
                _deleteButtonTooltip = value;
                NotifyOfPropertyChange(() => DeleteButtonTooltip);
            }
        }

        private string _refreshButtonTooltip;
        public string RefreshButtonTooltip
        {
            get { return _refreshButtonTooltip; }
            set
            {
                _refreshButtonTooltip = value;
                NotifyOfPropertyChange(() => RefreshButtonTooltip);
            }
        }

        private string _searchButtonTooltip;
        public string SearchButtonTooltip
        {
            get { return _searchButtonTooltip; }
            set
            {
                _searchButtonTooltip = value;
                NotifyOfPropertyChange(() => SearchButtonTooltip);
            }
        }

        private string _deletingText;
        public string DeletingText
        {
            get { return _deletingText; }
            set
            {
                _deletingText = value;
                NotifyOfPropertyChange(() => DeletingText);
            }
        }

        private string _dateText;
        public string DateText
        {
            get { return _dateText; }
            set
            {
                _dateText = value;
                NotifyOfPropertyChange(() => DateText);
            }
        }

        private string _recordsPerPageText;
        public string RecordsPerPageText
        {
            get { return _recordsPerPageText; }
            set
            {
                _recordsPerPageText = value;
                NotifyOfPropertyChange(() => RecordsPerPageText);
            }
        }

        private string _ofText;
        public string OfText
        {
            get { return _ofText; }
            set
            {
                _ofText = value;
                NotifyOfPropertyChange(() => OfText);
            }
        }

        private string _goToFirstPageText;
        public string GoToFirstPageText
        {
            get { return _goToFirstPageText; }
            set
            {
                _goToFirstPageText = value;
                NotifyOfPropertyChange(() => GoToFirstPageText);
            }
        }

        private string _goToPreviousPageText;
        public string GoToPreviousPageText
        {
            get { return _goToPreviousPageText; }
            set
            {
                _goToPreviousPageText = value;
                NotifyOfPropertyChange(() => GoToPreviousPageText);
            }
        }

        private string _goToNextPageText;
        public string GoToNextPageText
        {
            get { return _goToNextPageText; }
            set
            {
                _goToNextPageText = value;
                NotifyOfPropertyChange(() => GoToNextPageText);
            }
        }

        private string _goToLastPageText;
        public string GoToLastPageText
        {
            get { return _goToLastPageText; }
            set
            {
                _goToLastPageText = value;
                NotifyOfPropertyChange(() => GoToLastPageText);
            }
        }

        private string _pageText;
        public string PageText
        {
            get { return _pageText; }
            set
            {
                _pageText = value;
                NotifyOfPropertyChange(() => PageText);
            }
        }

        private string _currentPageText;
        public string CurrentPageText
        {
            get { return _currentPageText; }
            set
            {
                _currentPageText = value;
                NotifyOfPropertyChange(() => CurrentPageText);
            }
        }

        private string _lastPageText;
        public string LastPageText
        {
            get { return _lastPageText; }
            set
            {
                _lastPageText = value;
                NotifyOfPropertyChange(() => LastPageText);
            }
        }

        private string _restoreDefaultValuesText;
        public string RestoreDefaultValuesText
        {
            get { return _restoreDefaultValuesText; }
            set
            {
                _restoreDefaultValuesText = value;
                NotifyOfPropertyChange(() => RestoreDefaultValuesText);
            }
        }

        private string _objectDeletedSuccessfullyText;
        public string ObjectDeletedSuccessfullyText
        {
            get { return _objectDeletedSuccessfullyText; }
            set
            {
                _objectDeletedSuccessfullyText = value;
                NotifyOfPropertyChange(() => ObjectDeletedSuccessfullyText);
            }
        }

        private string _selectObjectToDeleteText;
        public string SelectObjectToDeleteText
        {
            get { return _selectObjectToDeleteText; }
            set
            {
                _selectObjectToDeleteText = value;
                NotifyOfPropertyChange(() => SelectObjectToDeleteText);
            }
        }

        private string _confirmDeleteText;
        public string ConfirmDeleteText
        {
            get { return _confirmDeleteText; }
            set
            {
                _confirmDeleteText = value;
                NotifyOfPropertyChange(() => ConfirmDeleteText);
            }
        }

        private string _deleteCanceledText;
        public string DeleteCanceledText
        {
            get { return _deleteCanceledText; }
            set
            {
                _deleteCanceledText = value;
                NotifyOfPropertyChange(() => DeleteCanceledText);
            }
        }

        private string _noRecordsInPageText;
        public string NoRecordsInPageText
        {
            get { return _noRecordsInPageText; }
            set
            {
                _noRecordsInPageText = value;
                NotifyOfPropertyChange(() => NoRecordsInPageText);
            }
        }

        private string _loadingRelatedObjectsText;
        public string LoadingRelatedObjectsText
        {
            get { return _loadingRelatedObjectsText; }
            set
            {
                _loadingRelatedObjectsText = value;
                NotifyOfPropertyChange(() => LoadingRelatedObjectsText);
            }
        }

        private string _relatedObjectsText;
        public string RelatedObjectsText
        {
            get { return _relatedObjectsText; }
            set
            {
                _relatedObjectsText = value;
                NotifyOfPropertyChange(() => RelatedObjectsText);
            }
        }

        private string _alreadyInFirstPageText;
        public string AlreadyInFirstPageText
        {
            get { return _alreadyInFirstPageText; }
            set
            {
                _alreadyInFirstPageText = value;
                NotifyOfPropertyChange(() => AlreadyInFirstPageText);
            }
        }

        private string _alreadyInLastPageText;
        public string AlreadyInLastPageText
        {
            get { return _alreadyInLastPageText; }
            set
            {
                _alreadyInLastPageText = value;
                NotifyOfPropertyChange(() => AlreadyInLastPageText);
            }
        }

        private string _importExcelButtonTooltip;
        public string ImportExcelButtonTooltip
        {
            get { return _importExcelButtonTooltip; }
            set
            {
                _importExcelButtonTooltip = value;
                NotifyOfPropertyChange(() => ImportExcelButtonTooltip);
            }
        }

        private string _importingFromExcelText;
        public string ImportingFromExcelText
        {
            get { return _importingFromExcelText; }
            set
            {
                _importingFromExcelText = value;
                NotifyOfPropertyChange(() => ImportingFromExcelText);
            }
        }

        private string _excelImportedSucessfullyText;
        public string ExcelImportedSucessfullyText
        {
            get { return _excelImportedSucessfullyText; }
            set
            {
                _excelImportedSucessfullyText = value;
                NotifyOfPropertyChange(() => ExcelImportedSucessfullyText);
            }
        }

		private string _loadingDefaultValuesText;
		public string LoadingDefaultValuesText
		{
			get { return _loadingDefaultValuesText; }
			set
			{
				_loadingDefaultValuesText = value;
				NotifyOfPropertyChange(() => LoadingDefaultValuesText);
			}
		}

		private string _additionalText;
		public string AdditionalText
		{
			get { return _additionalText; }
			set
			{
				_additionalText = value;
				NotifyOfPropertyChange(() => AdditionalText);
			}
		}

		private string _selectBusinessObjectToEditText;
		public string SelectBusinessObjectToEditText
		{
			get { return _selectBusinessObjectToEditText; }
			set
			{
				_selectBusinessObjectToEditText = value;
				NotifyOfPropertyChange(() => SelectBusinessObjectToEditText);
			}
		}

        private string _folioText;
        public string FolioText
        {
            get { return _folioText; }
            set
            {
                _folioText = value;
                NotifyOfPropertyChange(() => FolioText);
            }
        }

        private string _commentsText;
        public string CommentsText
        {
            get { return _commentsText; }
            set
            {
                _commentsText = value;
                NotifyOfPropertyChange(() => CommentsText);
            }
        }

        private string _totalRecordsInDatabaseToolTip;
        public string TotalRecordsInDatabaseToolTip
        {
            get { return _totalRecordsInDatabaseToolTip; }
            set
            {
                _totalRecordsInDatabaseToolTip = value;
                NotifyOfPropertyChange(() => TotalRecordsInDatabaseToolTip);
            }
        }

        private string _viewNameDoesNotEndsInViewText;
        public string ViewNameDoesNotEndsInViewText
        {
            get { return _viewNameDoesNotEndsInViewText; }
            set
            {
                _viewNameDoesNotEndsInViewText = value;
                NotifyOfPropertyChange(() => ViewNameDoesNotEndsInViewText);
            }
        }

        private string _popUpViewIsNotBasePopUpViewText;
        public string PopUpViewIsNotBasePopUpViewText
        {
            get { return _popUpViewIsNotBasePopUpViewText; }
            set
            {
                _popUpViewIsNotBasePopUpViewText = value;
                NotifyOfPropertyChange(() => PopUpViewIsNotBasePopUpViewText);
            }
        }

        private string _initializingPopUpText;
        public string InitializingPopUpText
        {
            get { return _initializingPopUpText; }
            set
            {
                _initializingPopUpText = value;
                NotifyOfPropertyChange(() => InitializingPopUpText);
            }
        }

        private string _loadingNewObjectPropertiesText;
        public string LoadingNewObjectPropertiesText
        {
            get { return _loadingNewObjectPropertiesText; }
            set
            {
                _loadingNewObjectPropertiesText = value;
                NotifyOfPropertyChange(() => LoadingNewObjectPropertiesText);
            }
        }

        private string _cloningSelectedBusinessObjectText;
        public string CloningSelectedBusinessObjectText
        {
            get { return _cloningSelectedBusinessObjectText; }
            set
            {
                _cloningSelectedBusinessObjectText = value;
                NotifyOfPropertyChange(() => CloningSelectedBusinessObjectText);
            }
        }

        private string _cloneSelectedBusinessObjectToolTip;
        public string CloneSelectedBusinessObjectToolTip
        {
            get { return _cloneSelectedBusinessObjectToolTip; }
            set
            {
                _cloneSelectedBusinessObjectToolTip = value;
                NotifyOfPropertyChange(() => CloneSelectedBusinessObjectToolTip);
            }
        }

        private string _cloneSelectedBusinessObjectsToolTip;
        public string CloneSelectedBusinessObjectsToolTip
        {
            get { return _cloneSelectedBusinessObjectsToolTip; }
            set
            {
                _cloneSelectedBusinessObjectsToolTip = value;
                NotifyOfPropertyChange(() => CloneSelectedBusinessObjectsToolTip);
            }
        }

        private string _selectBusinessObjectToCloneText;
        public string SelectBusinessObjectToCloneText
        {
            get { return _selectBusinessObjectToCloneText; }
            set
            {
                _selectBusinessObjectToCloneText = value;
                NotifyOfPropertyChange(() => SelectBusinessObjectToCloneText);
            }
        }

        private string _selectBusinessObjectsToCloneText;
        public string SelectBusinessObjectsToCloneText
        {
            get { return _selectBusinessObjectsToCloneText; }
            set
            {
                _selectBusinessObjectsToCloneText = value;
                NotifyOfPropertyChange(() => SelectBusinessObjectsToCloneText);
            }
        }

        private string _cloningObjectsText;
        public string CloningObjectsText
        {
            get { return _cloningObjectsText; }
            set
            {
                _cloningObjectsText = value;
                NotifyOfPropertyChange(() => CloningObjectsText);
            }
        }

        private string _cloneObjectsToolTip;
        public string CloneObjectsToolTip
        {
            get { return _cloneObjectsToolTip; }
            set
            {
                _cloneObjectsToolTip = value;
                NotifyOfPropertyChange(() => CloneObjectsToolTip);
            }
        }

        private string _businessObjectClonedSuccessfullyText;
        public string BusinessObjectClonedSuccessfullyText
        {
            get { return _businessObjectClonedSuccessfullyText; }
            set
            {
                _businessObjectClonedSuccessfullyText = value;
                NotifyOfPropertyChange(() => BusinessObjectClonedSuccessfullyText);
            }
        }

        private string _businessObjectsClonedSuccessfullyText;
        public string BusinessObjectsClonedSuccessfullyText
        {
            get { return _businessObjectsClonedSuccessfullyText; }
            set
            {
                _businessObjectsClonedSuccessfullyText = value;
                NotifyOfPropertyChange(() => BusinessObjectsClonedSuccessfullyText);
            }
        }

        private string _businessObjectClonationCancelledText;
        public string BusinessObjectClonationCancelledText
        {
            get { return _businessObjectClonationCancelledText; }
            set
            {
                _businessObjectClonationCancelledText = value;
                NotifyOfPropertyChange(() => BusinessObjectClonationCancelledText);
            }
        }

        #endregion Multilanguage

        #endregion Properties

        #region Commands

        #region About Command

        public ICommand AboutCommand { get; set; }

        private void AboutCommandAction()
        {
            DisplayAboutView();
        }

        #endregion Search Command

        #region Search Command

        public ICommand SearchCommand { get; set; }

        private async void SearchCommandAction()
        {
            await SearchBusinessObjects(TextForSearch);
        }

        #endregion Search Command

        #region Search Command of the Magnifying Glass of the SearchButton User Control

        /// <summary>
        /// Command executed when the Magnifying Glass of the SearchButton User Control is clicked.
        /// This Command must be different than the Seach Command, because if the User clicks when the Button is an X,
        /// there is no need to Search the Business Objects, only to Clear the Text.
        /// </summary>
        public ICommand MagnifyingGlassCommand { get; set; }

        private async void MagnifyingGlassCommandAction()
        {
            if (string.IsNullOrEmpty(TextForSearch))
            {
                await SearchBusinessObjects(TextForSearch);
            }
        }

        #endregion Search Command of the Magnifying Glass of the SearchButton User Control

        #region New Business Object Command

        public ICommand NewBusinessObjectCommand { get; set; }

        /// <summary>
        /// Method executed when the New Business Object Button is Clicked in the ToolBar.
        /// This method can't be virtual, because we always want to Check the Permissions of the User
        /// and we don't want that any Child Class override that. Child classes can override the
        /// NewBusinessObjectMethod, that is executed after the Permissions Check.
        /// </summary>
        private async void NewBusinessObjectAction()
        {
			await NewBusinessObjectActionAsync();
        }

		private async Task NewBusinessObjectActionAsync()
		{
			if (!await CheckUserPermissions(PermissionTypeEnum.Create))
			{
				return;
			}

			await NewBusinessObjectMethod();

            if (CreateEditBusinessObjectsInPopUps)
            {
                await OpenPopUp(NewBusinessObject);
            }
        }

        public virtual async Task NewBusinessObjectMethod()
        {
			try
			{
				SetViewIsBusy(LoadingDefaultValuesText);

				ShowNewBusinessObjectFlyOut = true;
				NewBusinessObject = await Manager.GetNewBusinessObject();
			}
			catch(Exception e)
			{
                ShowErrorMessage(e);
			}
			finally
			{
				ViewIsBusy = false;
			}
        }

        protected virtual async Task OpenPopUp(T businessObject)
        {
            try
            {
                var message = string.Empty;

                if(businessObject == null || businessObject.Id <= 0)
                {
                    message = LoadingNewObjectPropertiesText;
                }
                else
                {
                    message = LoadingPropertiesText;
                }

                SetViewIsBusy(message);
                //SetViewIsBusy(InitializingPopUpText);

                await ManagerUtil.DelayTask();

                BasePopUpView popUpView = null;

                if (PopUpViewType == null)
                {
                    //Try to create an instance of a View named the same than this View, but with the suffix PopUp.
                    var indexOf = ViewName.IndexOf(BaseConstants.View);

                    if (indexOf < 0)
                    {
                        message = string.Format(ViewNameDoesNotEndsInViewText, ViewName);
                        ShowErrorMessage(message);
                        return;
                    }

                    var popUpViewName = ViewName.Substring(0, indexOf);
                    popUpViewName += BaseConstants.PopUp + BaseConstants.View;

                    var assemblyName = BaseConstants.AssemblyName;

                    popUpView = ReflectionUtil.CreateInstance<dynamic>(assemblyName, popUpViewName);

                    if (popUpView == null)
                    {
                        message = string.Format(CantCreateInstanceText, popUpViewName, assemblyName);
                        ShowErrorMessage(message);
                        return;
                    }

                    PopUpViewType = popUpView.GetType();
                    PopUpAssemblyName = assemblyName;
                }
                else if (!ReflectionUtil.TypeHaveBaseType(PopUpViewType, typeof(BasePopUpView)))
                {
                    message = string.Format(PopUpViewIsNotBasePopUpViewText, PopUpViewType.FullName, typeof(BasePopUpView).FullName);
                    ShowErrorMessage(message);
                    return;
                }
                else if (PopUpViewType != null)
                {
                    popUpView = ReflectionUtil.CreateInstance<dynamic>(PopUpAssemblyName, PopUpViewType.FullName);

                    if (popUpView == null)
                    {
                        message = string.Format(CantCreateInstanceText, PopUpViewType.FullName, PopUpAssemblyName);
                        ShowErrorMessage(message);
                        return;
                    }
                }

                popUpView.MenuItem = MenuItem;
                popUpView.SelectedBusinessObject = businessObject;

                InitializeView(popUpView, true, false, false, true);
            }
            catch(Exception e)
            {
                ShowErrorMessage(e);
            }
            finally
            {
                ViewIsBusy = false;
            }
        }

        #endregion New Business Object Command

        #region Cancel Command

        public ICommand CancelCommand { get; set; }

        private void CancelCommandAction()
        {
            Cancel();
        }

        #endregion Cancel Command

        #region Delete Business Object Command

        /// <summary>
        /// Command for Deleting a Business Object.
        /// </summary>
        public ICommand DeleteBusinessObjectCommand { get; set; }

        private async void DeleteBusinessObjectCommandAction()
        {
			await DeleteBusinessObjectCommandActionAsync();
        }

		private async Task DeleteBusinessObjectCommandActionAsync()
		{
			if (!await CheckUserPermissions(PermissionTypeEnum.Delete))
			{
				return;
			}

			await DeleteBusinessObject();
		}

        #endregion Delete Business Object Command

        #region Go To First Page Command

        /// <summary>
        /// Command to Go To the First Page.
        /// </summary>
        public ICommand GoToFirstPageCommand { get; set; }

        private async void GoToFirstPageCommandAction()
        {
            await GoToFirstPageAction();
        }

        #endregion Go To First Page Command

        #region Go To Previous Page Command

        /// <summary>
        /// Command to Go To the Previous Page.
        /// </summary>
        public ICommand GoToPreviousPageCommand { get; set; }

        private async void GoToPreviousPageCommandAction()
        {
            await GoToPreviousPageAction();
        }

        #endregion Go To Previous Page Command

        #region Go To Next Page Command

        /// <summary>
        /// Command to Go To the Next Page.
        /// </summary>
        public ICommand GoToNextPageCommand { get; set; }

        private async void GoToNextPageCommandAction()
        {
            await GoToNextPageAction();
        }

        #endregion Go To Next Page Command

        #region Go To Last Page Command

        /// <summary>
        /// Command to Go To the Last Page.
        /// </summary>
        public ICommand GoToLastPageCommand { get; set; }

        private async void GoToLastPageCommandAction()
        {
            await GoToLastPageAction();
        }

        #endregion Go To Last Page Command

        #region Restore Search Default Values Command

        /// <summary>
        /// Command to Restore the Default Values of the Search.
        /// </summary>
        public ICommand RestoreSearchDefaultValuesCommand { get; set; }

        private async void RestoreSearchDefaultValuesAction()
        {
			await RestoreSearchDefaultValuesActionAsync();
        }

		private async Task RestoreSearchDefaultValuesActionAsync()
		{
			RestoreSearchDefaultValues();
			await SearchBusinessObjects();
		}

		#endregion Restore Search Default Values Command

		#region SearchTextBox Command

		public override void SearchTextBoxCommandAction()
        {
            if(string.IsNullOrEmpty(TextForSearch))
            {
                SearchCommandAction();
            }
        }

        #endregion SearchTextBox Command

        #region Import Excel Command

        public ICommand ImportExcelCommand { get; set; }

        private async void ImportExcelCommandAction()
        {
            await ImportFromExcel();
        }

		#endregion Import Excel Command

		#region OpenImportDocumentView Command

		/// <summary>
		/// Command to Show the Import Document View.
		/// </summary>
		public ICommand OpenImportDocumentViewCommand { get; set; }

		#endregion OpenImportDocumentView Command

		#region EditBusinessObject Command

		/// <summary>
		/// Command to Edit the Selected Business Object.
		/// </summary>
		public ICommand EditBusinessObjectCommand { get; set; }

		protected async void EditBusinessObjectAction()
		{
			if(SelectedBusinessObject == null)
			{
				ShowFlyOutMessage(SelectBusinessObjectToEditText);
				return;
			}

			await ShowEditTab();
		}

		public virtual async Task ShowEditTab()
		{
			ShowEditBusinessObjectTab = true;
			await EditBusinessObjectActionAsync();
		}

		public virtual async Task EditBusinessObjectActionAsync()
		{
            if (CreateEditBusinessObjectsInPopUps)
            {
                await OpenPopUp(SelectedBusinessObject);
            }
            else
            {
                await AfterSelectBusinessObjectAsync();
            }
		}

        #endregion EditBusinessObject Command

        #region CloneSelectedBusinessObjects Command

        /// <summary>
        /// Command to Clone the Selected Business Object.
        /// </summary>
        public ICommand CloneSelectedBusinessObjectsCommand { get; set; }

        protected async void CloneSelectedBusinessObjectsAction()
        {
            if(BusinessObjects == null)
            {
                return;
            }

            var selectedBusinessObjectsCount = BusinessObjects.Count(x => x.IsSelected);

            if (selectedBusinessObjectsCount <= 0 && SelectedBusinessObject == null)
            {
                ShowFlyOutMessage(SelectBusinessObjectToCloneText);
                return;
            }

            if (selectedBusinessObjectsCount <= 1 && SelectedBusinessObject != null)
            {
                await CloneSelectedBusinessObject();
            }
            else
            {
                //Always ask before cloning Multiple Objects.
                //if (SessionVariables.Instance.UserPreferences != null && SessionVariables.Instance.UserPreferences.AskBeforeSave)
                {
                    var message = await GetText("ConfirmCloneObjects");

                    if (Confirm(message) != MessageBoxGeneralResult.Yes)
                    {
                        ShowFlyOutMessage(BusinessObjectClonationCancelledText);
                        return;
                    }
                }

                var properties = ReflectionUtil.GetPropertiesInfo<T>();
                var codeProperty = properties.FirstOrDefault(x => x.Name == BaseConstants.Code && 
                !ReflectionUtil.HaveAttribute(x, typeof(ExcludeMappingAttribute)));

                var nameProperty = properties.FirstOrDefault(x => x.Name == BaseConstants.Name &&
                !ReflectionUtil.HaveAttribute(x, typeof(ExcludeMappingAttribute)));

                var excludedProperties = new List<string> { BaseConstants.Id, BaseConstants.Code };
                var success = true;

                foreach (var selectedBusinessObject in BusinessObjects.Where(x => x.IsSelected))
                {
                    success = await CloneAndInsert(selectedBusinessObject, excludedProperties, properties, codeProperty, nameProperty);

                    if(!success)
                    {
                        break;
                    }
                }

                await SearchBusinessObjects();

                if(success)
                {
                    ShowFlyOutMessage(BusinessObjectsClonedSuccessfullyText);
                }
            }
        }

        /// <summary>
        /// Clone the Selected Business Object (Only one Business Object).
        /// Opens the CloneObjectView.
        /// </summary>
        /// <returns>Task.</returns>
        protected async Task CloneSelectedBusinessObject()
        {
            try
            {
                SetViewIsBusy(WaitingUntilPopUpIsClosedText);

                var cloneObjectView = new CloneObjectView
                {
                    DataContext = new CloneObjectViewModel<T>()
                };

                cloneObjectView.Initialize();

                var dataContext = cloneObjectView.DataContext as CloneObjectViewModel<T>;
                dataContext.SelectedBusinessObject = BusinessObjects.FirstOrDefault(x => x.IsSelected);

                if (dataContext.SelectedBusinessObject == null)
                {
                    dataContext.SelectedBusinessObject = SelectedBusinessObject;
                }

                dataContext.CurrentManager = Manager;

                InitializeView(cloneObjectView, false, false, false, true);

                if (dataContext.Success)
                {
                    ShowFlyOutMessage(BusinessObjectClonedSuccessfullyText);
                    await SearchBusinessObjects();
                }
                else if (!dataContext.IsAcceptButtonClicked)
                {
                    ShowFlyOutMessage(BusinessObjectClonationCancelledText);
                }
            }
            catch(Exception e)
            {
                ShowErrorMessage(e);
            }
            finally
            {
                ViewIsBusy = false;
            }
        }

        /// <summary>
        /// Clones the specified Business Object and Insert it into the Database.
        /// </summary>
        /// <param name="businessObject">Business Object to be cloned and inserted.</param>
        /// <param name="properties">List of Properties of the Business Object.</param>
        /// <param name="codeProperty">Code Property of the Business Object.
        /// This is used to set the New Code of the Business Object.</param>
        /// <param name="nameProperty">Name Property of the Business Object.</param>
        /// <returns>Boolean value indicating if the Business Object was clonated and inserted successfully.</returns>
        protected async Task<bool> CloneAndInsert(T businessObject, List<string> excludedProperties, 
            List<PropertyInfo> properties, PropertyInfo codeProperty, PropertyInfo nameProperty)
        {
            try
            {
                var message = await GetText("CloningObject");
                var code = codeProperty == null ? null : codeProperty.GetValue(businessObject);

                if (code != null && !string.IsNullOrEmpty(code.ToString()))
                {
                    message = string.Format(message, code);
                }
                else
                {
                    var name = nameProperty == null ? null : nameProperty.GetValue(businessObject);

                    if (name != null && !string.IsNullOrEmpty(name.ToString()))
                    {
                        message = string.Format(message, name);
                    }
                }

                SetViewIsBusy(message);

                var response = await Manager.CloneBusinessObject(businessObject, excludedProperties, properties, codeProperty);

                if(!response.Success)
                {
                    return false;
                }

                var cloneObject = response.Object;
                var insertResponse = await Manager.Insert(cloneObject);

                if (!ProcessResponse(insertResponse))
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);
                return false;
            }
            finally
            {
                ViewIsBusy = false;
            }
        }

        #endregion CloneSelectedBusinessObjects Command

        #region SelectAllBusinessObjects Command

        /// <summary>
		/// Command executed when the Select All Business Objects CheckBox is Clicked.
		/// </summary>
		public ICommand SelectAllBusinessObjectsCommand { get; set; }

        protected void SelectAllBusinessObjectsAction(object isChecked)
        {
            _allBusinessObjectsSelected = Manager.SelectAllBusinessObjects(BusinessObjects, isChecked);

            NotifyOfPropertyChange(() => AllBusinessObjectsSelected);
        }

        protected virtual void NotifyAllSelectedBusinessObjects()
        {
            _allBusinessObjectsSelected = BusinessObjects != null && BusinessObjects.All(x => x.IsSelected);

            NotifyOfPropertyChange(() => AllBusinessObjectsSelected);
        }

        #endregion SelectAllBusinessObjects Command

        #region CheckedBusinessObject Command

        /// <summary>
		/// Command executed when the CheckBox of a Business Object Row in the Datagrid is checked.
		/// </summary>
		public ICommand CheckedBusinessObjectCommand { get; set; }

        protected void CheckedBusinessObjectAction(object isChecked)
        {
            if (isChecked is T)
            {
                var businessObject = isChecked as T;
                businessObject.IsSelected = true;
            }

            _allBusinessObjectsSelected = Manager.SelectAllBusinessObjects(BusinessObjects, isChecked);

            NotifyOfPropertyChange(() => AllBusinessObjectsSelected);
        }

        #endregion CheckedBusinessObject Command

        #region UnCheckedBusinessObject Command

        /// <summary>
		/// Command executed when the CheckBox of a Business Object Row in the Datagrid is unchecked.
		/// </summary>
		public ICommand UnCheckedBusinessObjectCommand { get; set; }

        protected void UnCheckedBusinessObjectAction(object isChecked)
        {
            if (isChecked is T)
            {
                var businessObject = isChecked as T;
                businessObject.IsSelected = false;
            }

            _allBusinessObjectsSelected = Manager.SelectAllBusinessObjects(BusinessObjects, isChecked);

            NotifyOfPropertyChange(() => AllBusinessObjectsSelected);
        }

        #endregion UnCheckedBusinessObject Command

        #endregion Commands

        #region Methods

        public virtual void AssignCancelButtonText()
        {
            if (NewBusinessObject == null && !ShowEditBusinessObjectTab)
            {
                CancelText = CloseText;
                CancelButtonToolTip = CloseButtonToolTipText;
            }
            else
            {
                CancelText = GetSystemText("CancelText", BaseConstants.Cancel);
                CancelButtonToolTip = GetSystemText("CancelButtonToolTip", BaseConstants.Cancel);
            }
        }

        /// <summary>
        /// Method executed when the Cancel Button is Clicked.
        /// </summary>
        public virtual void Cancel()
        {
			//If we are creating a New Business Object or Editing it, the Cancel Button will cancel the
			//Creation/Edition, unless we want to create / Edit the Business Object in a PopUp. In that case, we close the Search Window.
            if (!CreateEditBusinessObjectsInPopUps && (ShowNewBusinessObjectFlyOut || ShowEditBusinessObjectTab))
            {
                ShowNewBusinessObjectFlyOut = false;
				ShowEditBusinessObjectTab = false;
			}
            else
            {
                CloseWindowAction();
            }
        }

        /// <summary>
        /// Method executed before changing the Selected Business Object.
        /// </summary>
        public virtual void BeforeSelectBusinessObject()
        { }

        /// <summary>
        /// Restore the Default Values of the Search.
        /// </summary>
        public virtual void RestoreSearchDefaultValues()
        {
            if (SessionVariables.Instance.UserPreferences != null)
            {
                RecordsPerPage = SessionVariables.Instance.UserPreferences.RecordsPerPage.ToString();
            }
            else
            {
                RecordsPerPage = "1000";
            }

            CurrentPage = "1";
        }

        /// <summary>
        /// Method executed after a Business Object is Selected in the DataGrid.
        /// We make this method private, because we want to execute always this method as it contains the logic to
        /// wait before loading the Selected Business Object when the View
        /// is Busy. We let child classes to override the AfterSelectBusinessObject method.
        /// </summary>
        private async void AfterSelectBusinessObjectAction()
        {
			//If the View wants to show the Edit button, then the Properties of the Selected Business Object
			//are being loaded when the Edit Button is clicked, and not when the Business Object is Selected.
			if(CanEdit || CanViewDetail)
			{
                //CheckSelectedBusinessObject();
                return;
			}

            await AfterSelectBusinessObjectAsync();
        }

        public virtual async Task AfterSelectBusinessObjectAsync()
        {
            if(CreateEditBusinessObjectsInPopUps)
            {
                //CheckSelectedBusinessObject();
                return;
            }

            try
            {
                if (SelectedBusinessObject == null || BusinessObjects == null || ViewIsBusy)
                {
                    return;
                }

				if(SelectedTimer != null)
				{
					SelectedTimer.Stop();
				}

                SetViewIsBusy(LoadingPropertiesText);
                IsLoadingProperties = true;

                //When the User Select the Business Object in the DataGrid we want to Get
                //all of the Properties of the Business Object, to display its Values.

                //We don't assign directly the SelectedBusinessObject, because it will change the reference of the Object,
                //and if the DataSource is a TreeView, this can throw an error. We copy all of the properties.
                if (SelectedBusinessObject.Id > 0)
                {
                    var selectedBusinessObjectLoaded = BusinessObjects.FirstOrDefault(x => x.Id == SelectedBusinessObject.Id);

                    if(selectedBusinessObjectLoaded != null)
                    {
                        _selectedBusinessObject = selectedBusinessObjectLoaded;
                    }

                    NotifyOfPropertyChange(() => SelectedBusinessObject);

                    var loadByIdParameter = new LoadByIdParameter
                    {
                        Id = SelectedBusinessObject.Id,
                        CompanyId = SelectedBusinessObject.CompanyId,
                        FullLoad = FullLoadBusinessObject
                    };

                    var businessObject = SelectedBusinessObject as BusinessObject;
                    if(businessObject != null && !businessObject.Active)
                    {
                        loadByIdParameter.ActiveStatus = ActiveStatusEnum.ActivesInactives;
                    }

                    var response = await Manager.GetByIdWithParameters(loadByIdParameter);

                    if(!ProcessResponse(response))
                    {
                        return;
                    }

                    selectedBusinessObjectLoaded = response.Object;

					SetViewIsBusy(LoadingPropertiesText);
					ReflectionUtil.CopyTo(selectedBusinessObjectLoaded, _selectedBusinessObject);

                    _selectedBusinessObject.HaveChanges = false;
                }

				NotifyOfPropertyChange(() => SelectedBusinessObjectNotNull);

                AfterSelectBusinessObject = false;
                AfterSelectBusinessObject = true;
				LoadBusinessObjectToEdit = false;

				if (ShowEditBusinessObjectTab)
				{
					LoadBusinessObjectToEdit = true;
				}

                NotifyBusinessObjectMultiLanguage();

                //CheckSelectedBusinessObject();
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);
            }
            finally
            {
                ViewIsBusy = false;
                IsLoadingProperties = false;
            }
        }

        private void CheckSelectedBusinessObject()
        {
            if(SelectedBusinessObject != null)
            {
                SelectedBusinessObject.IsSelected = true;
            }
            
            NotifyAllSelectedBusinessObjects();
        }

        /// <summary>
        /// Loads the specified Business Object in the View.
        /// </summary>
        /// <param name="businessObject">Business Object to be loaded.</param>
        public virtual void LoadBusinessObject(T businessObject)
        {
            SelectedBusinessObject = businessObject;
        }

        /// <summary>
        /// Gets the Business Objects that Have Changes.
        /// A Business Object Have Changes if:
        /// 1- The Business Object have changes or
        /// 2- Any of its Child Business Object have changes.
        /// </summary>
        /// <returns>List of Business Objects with changes.</returns>
        public virtual List<T> GetChangedBusinessObjects()
        {
            var changedBusinessObjects = new List<T>();

            if(BusinessObjects == null)
            {
                return changedBusinessObjects;
            }

            foreach(var businessObject in BusinessObjects)
            {
                if(businessObject.BusinessObjectHaveChanges())
                {
                    changedBusinessObjects.Add(businessObject);
                }
            }

            return changedBusinessObjects;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
                              /// Validates a Business Object before being saved into the Database.
                              /// This Validations are performed in ViewModels, before going to the
                              /// Validations in the Repositories, because they may need some
                              /// User Confirmation, and the Confirmation Window must be displayed
                              /// from ViewModels.
                              /// </summary>
                              /// <param name="businessObject">Business Object to Be Validated.</param>
                              /// <returns></returns>
        public virtual async Task<bool> ValidateBusinessObject(T businessObject)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return await ValidateBusinessObject<T>(businessObject);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		/// <summary>
		/// Validates a Business Object before being saved into the Database.
		/// This Validations are performed in ViewModels, before going to the
		/// Validations in the Repositories, because they may need some
		/// User Confirmation, and the Confirmation Window must be displayed
		/// from ViewModels.
		/// </summary>
		/// <param name="businessObject">Business Object to Be Validated.</param>
		/// <returns></returns>
		public virtual async Task<bool> ValidateBusinessObject<O>(O businessObject)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
		{
			return true;
		}

		/// <summary>
		/// Insert or Update the Selected Business Object.
		/// </summary>
		public override async Task<bool> InsertOrUpdate(bool neverAsk = false)
        {
            var success = await base.InsertOrUpdate(neverAsk);

            if(!success)
            {
                return success;
            }

            try
            {
                if(NewBusinessObject == null && SelectedBusinessObject == null)
                {
                    ShowFlyOutMessage(SelectObjectToSaveText);
                    return false;
                }

                if(SessionVariables.Instance.UserPreferences != null
                    && SessionVariables.Instance.UserPreferences.AskBeforeSave
                    && !neverAsk)
                {
                    if(Confirm(ConfirmSaveBusinessObjectText) != MessageBoxGeneralResult.Yes)
                    {
                        return false;
                    }
                }

                SetViewIsBusy(SavingText);
                BaseResponse response;
                var nullBusinessObject = true;

                if(NewBusinessObject != null)
                {
                    nullBusinessObject = false;
                    if (!await ValidateBusinessObject(NewBusinessObject))
                    {
                        return false;
                    }

                    response = await Manager.InsertOrUpdate(NewBusinessObject);

                    if (!ProcessResponse(response))
                    {
                        return false;
                    }

                    //We search only if we added a New Business Object.
                    //await SearchBusinessObjects(TextForSearch);
                }

                //Save every Business Object with Changes, not only the Selected Business Object.
                var changedBusinessObjects = GetChangedBusinessObjects();

                if((changedBusinessObjects == null || !changedBusinessObjects.Any()) && nullBusinessObject)
                {
                    ShowFlyOutMessage(NoChangesText);
                    return false;
                }

                foreach(var businessObject in changedBusinessObjects)
                {
                    if(!await ValidateBusinessObject(businessObject))
                    {
                        return false;
                    }
                }

                response = await Manager.InsertOrUpdateList(changedBusinessObjects);

                if (!ProcessResponse(response))
                {
                    return false;
                }
                else
                {
                    //Hide the FlyOut only if the Save was successfull.
                    ShowNewBusinessObjectFlyOut = false;

                    ShowFlyOutMessage(InformationSavedSuccessfullyText);
                }

                //Always search the Business Objects to get the Information in the Database.
                await SearchBusinessObjects(TextForSearch);
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);

                success = false;
            }
            finally
            {
                ViewIsBusy = false;
            }

            return success;
        }

        public override bool HaveChanges()
        {
            return BusinessObjects != null && BusinessObjects.Any(x => x.HaveChanges);
        }

		public virtual MessageBoxGeneralResult ConfirmDeleteBusinessObject()
		{
			return Confirm(ConfirmDeleteText, "", ButtonsBar.MessageBoxButtonEnum.YesNo);
		}

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public virtual async Task<bool> ValidateBeforeDelete()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
		{
			if(SelectedBusinessObject == null)
            {
                ShowFlyOutMessage(SelectObjectToDeleteText);
                return false;
            }

            if(ConfirmDeleteBusinessObject() != MessageBoxGeneralResult.Yes)
            {
				ShowFlyOutMessage(DeleteCanceledText);
				return false;
            }

			return true;
		}

        /// <summary>
        /// Deletes the Selected Business Object.
        /// </summary>
        public virtual async Task DeleteBusinessObject()
        {
            try
            {
                if(!await ValidateBeforeDelete())
				{
					return;
				}

                SetViewIsBusy(DeletingText);
                await Manager.Delete(SelectedBusinessObject);

                ShowFlyOutMessage(ObjectDeletedSuccessfullyText);

                ViewIsBusy = false;
                await SearchBusinessObjects(TextForSearch);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("It exists in") || e.Message.Contains("It exists on"))
                {
                    ViewIsBusy = false;
                    if (Confirm(RelatedObjectsText) == MessageBoxGeneralResult.Yes)
                    {
                        await LoadRelatedObjects(SelectedBusinessObject);
                    }
                }
                else
                {
                    ShowErrorMessage(e);
                }
            }
            finally
            {
                ViewIsBusy = false;
            }
        }

        /// <summary>
        /// Loads the Related Objects of the specified Object.
        /// Related Objects are the Objects with Foreign Keys to this Object.
        /// </summary>
        /// <param name="businessObject">Business Object.</param>
        /// <returns>Related Objects to the specified Object.</returns>
        public virtual async Task LoadRelatedObjects(T businessObject)
        {
            try
            {
                SetViewIsBusy(LoadingRelatedObjectsText);

                var relatedBusinessObjects = await Manager.GetRelatedBusinessObjects(businessObject);

                var properties = ReflectionUtil.GetPropertiesInfo(typeof(BusinessObject));

                //Export Related Objects to Excel.
                var excelUtil = new ExcelUtil
                {
                    ViewModelBase = this
                };

				//When Loading Related Business Objects we don't check if the User have Permission to Export
				//to Excel, because the Permission is in a specific Menu, but when Loading Related Business Objects
				//there is no Menu Item.
                await excelUtil.ExportToExcel(relatedBusinessObjects, true, true, false);
            }
            catch(Exception e)
            {
                ShowErrorMessage(e);
            }
            finally
            {
                ViewIsBusy = false;
            }
        }

        public override async Task Initialize()
        {
            //In Design Mode, do nothing.
            if(DesignerProperties.GetIsInDesignMode(DependObject) || IsLoaded)
            {
                return;
            }

            if (!ViewIsBusy && !SessionVariables.Instance.IsOffline)
            {
                try
                {
                    await base.Initialize();

					if (SessionVariables.Instance.UserPreferences != null && SessionVariables.Instance.UserPreferences.SearchOnWindowOpen)
					{
						await SearchBusinessObjects();
					}
                    //base.LoadedEventMethod();
                }
                catch(Exception e)
                {
                    ShowErrorMessage(e);
                }
            }
            else if(SessionVariables.Instance.IsOffline)
            {
                SetTexts();
            }

            SetActiveStatusVisibility(BusinessObjectProperties);
            SetCreatedDateVisibility(BusinessObjectProperties);

            IsLoaded = true;
        }

        protected override void SetToolBoxButtonsVisibility()
        {
            base.SetToolBoxButtonsVisibility();

            //If we want to Create / Edit Business Objects in PopUps:
            //1- Hide the Save Button, because we can only modify the Business Object in the PopUp, not in the View.
            //2- Hide the Refresh Button for the same previous reason.
            //3- Hide the Cancel Button for the same previous reason.
            if (CreateEditBusinessObjectsInPopUps)
            {
                CanEdit = true;
                CanSave = false;
                CanRefresh = false;
                CanClear = false;
            }
        }

        /// <summary>
        /// Sets the Visibility of the ActiveStatus ComboBox for Searching Active and Inactives Objects.
        /// If the Object does not have the Active Property, the ActiveStatus ComboBox will be Collapsed.
        /// </summary>
        protected virtual void SetActiveStatusVisibility(List<PropertyInfo> properties)
		{
            //Check if the Business Object have the Active Property.
			var activeProperty = properties.FirstOrDefault(x => x.Name == BaseConstants.Active);

			ActiveStatusVisibility = activeProperty == null ? Visibility.Collapsed : Visibility.Visible;
		}

        /// <summary>
		/// Sets the Visibility of the Created Date for Searching Objects by its Creation Date.
		/// If the Object does not have the CreatedDate Property, the Created Date Search will be Collapsed.
		/// </summary>
		protected virtual void SetCreatedDateVisibility(List<PropertyInfo> properties)
        {
            //Check if the Business Object have the Created Date Property.
            var createdDateProperty = properties.FirstOrDefault(x => x.Name == BaseConstants.CreatedDate);

            CreatedDateVisibility = createdDateProperty == null ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Searches Business Objects.
        /// </summary>
        /// <param name="text">Search Text.</param>
        /// <returns>List of Business Objects.</returns>
        public override async Task<bool> SearchBusinessObjects(string text = "")
        {
            try
            {
                SetViewIsBusy(SearchingText);

				var searchParameter = GetSearchParameters(text);

                if(searchParameter == null || Manager == null)
                {
                    return false;
                }

                var businessObjectsTuple = await Manager.SearchWithCount(searchParameter);

                BusinessObjects = businessObjectsTuple.Item1;
                BusinessObjectsTotalCount = businessObjectsTuple.Item2;

                EndPage = Manager.GetEndPage(searchParameter, BusinessObjectsTotalCount);

                if (SelectedBusinessObject != null)
                {
                    IsLoadingProperties = true;
                    ViewIsBusy = false;
                    SelectedBusinessObject = BusinessObjects.FirstOrDefault(x => x.Id == SelectedBusinessObject.Id);
                }

                if (!BusinessObjects.Any())
                {
                    var message = NoRecordsInPageText;
                    message = string.Format(message, CurrentPage);

                    ShowFlyOutMessage(message);
                }

                return true;
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);
                return false;
            }
            finally
            {
                ViewIsBusy = false;
                IsLoadingProperties = false;
            }
        }
        
        public virtual async Task GoToFirstPageAction()
        {
            if(CurrentPage == "1")
            {
                ShowFlyOutMessage(AlreadyInFirstPageText);
                return;
            }

            GoToFirstPage = true;
            await SearchBusinessObjects(TextForSearch);
            GoToFirstPage = false;
            CurrentPage = "1";
        }

        /// <summary>
        /// Goes to the Previus Page of the Search.
        /// </summary>
        /// <returns></returns>
        public virtual async Task GoToPreviousPageAction()
        {
            if (CurrentPage == "1")
            {
                ShowFlyOutMessage(AlreadyInFirstPageText);
                return;
            }

            var currentPage = int.Parse(CurrentPage);
            if (currentPage > 1)
            {
                currentPage--;
                CurrentPage = currentPage.ToString();
            }

            await SearchBusinessObjects(TextForSearch);
        }

        /// <summary>
        /// Goes to the Next Page of the Search.
        /// </summary>
        /// <returns></returns>
        public virtual async Task GoToNextPageAction()
        {
            var previousEndPage = EndPage;

            if (!BusinessObjects.Any())
            {
                var message = NoRecordsInPageText;
                message = string.Format(message, CurrentPage);

                ShowFlyOutMessage(message);
                return;
            }

            var currentPage = int.Parse(CurrentPage);
            if (currentPage >= 1)
            {
                currentPage++;
                CurrentPage = currentPage.ToString();
            }

            var oldBusinessObjects = BusinessObjects;
            await SearchBusinessObjects(TextForSearch);

            if(BusinessObjectsCount <= 0)
            {
                currentPage--;
                CurrentPage = currentPage.ToString();
                BusinessObjects = oldBusinessObjects;
                EndPage = previousEndPage;
            }
        }

        /// <summary>
        /// Goes to the Last Page of the Search.
        /// </summary>
        /// <returns></returns>
        public virtual async Task GoToLastPageAction()
        {
            if (!BusinessObjects.Any())
            {
                var message = NoRecordsInPageText;
                message = string.Format(message, CurrentPage);

                ShowFlyOutMessage(message);
                return;
            }

            GoToLastPage = true;
            await SearchBusinessObjects(TextForSearch);
            GoToLastPage = false;

            var recordsPerPage = int.Parse(RecordsPerPage);
            var currentPage = string.Empty;

            if (BusinessObjectsTotalCount % recordsPerPage != 0)
            {
                currentPage = EndPage = ((BusinessObjectsTotalCount / recordsPerPage) + 1).ToString();
            }
            else
            {
                currentPage = EndPage = (BusinessObjectsTotalCount / recordsPerPage).ToString();
            }

            if(currentPage == CurrentPage)
            {
                var message = AlreadyInLastPageText;
                message = string.Format(message, currentPage);
                ShowFlyOutMessage(message);
                return;
            }

            CurrentPage = currentPage;
        }

        public override async Task RefreshInformationAsync()
        {
            if(ViewIsBusy)
            {
                return;
            }

            try
            {
                SetViewIsBusy(RefreshingText);

                if (SelectedBusinessObject != null)
                {
                    var response = await Manager.GetById(SelectedBusinessObject.Id);

                    if(!ProcessResponse(response))
                    {
                        return;
                    }

                    var businessObject = response.Object;
                    ReflectionUtil.CopyTo(businessObject, SelectedBusinessObject);
                    SelectedBusinessObject.HaveChanges = false;
                }
                else
                {
                    await SearchBusinessObjects(TextForSearch);
                }

                AfterRefreshInformation();
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);
            }
            finally
            {
                ViewIsBusy = false;
            }
        }

        /// <summary>
        /// Method executed after the RefreshInformation Method was executed.
        /// The RefreshInformation Method is executed when the Refresh Button of the 
        /// ToolBar is clicked.
        /// </summary>
        public virtual void AfterRefreshInformation()
        {
            NotifyBusinessObjectMultiLanguage();
        }

        /// <summary>
        /// Clear the Information in the View.
        /// </summary>
        public override async Task ClearAsync()
        {
            SelectedBusinessObject = null;

            if (ShowNewBusinessObjectFlyOut)
            {
                NewBusinessObject = await Manager.GetNewBusinessObject();
            }
            else
            {
                NewBusinessObject = null;
            }

            TextForSearch = string.Empty;
            MinimumCreationDate = null;
            MaximumCreationDate = null;
            ActiveStatus = ActiveStatusEnum.Actives;
        }

        public override void NotifyBusinessObjectMultiLanguage()
        {
            base.NotifyBusinessObjectMultiLanguage();
            NotifyOfPropertyChange(() => SelectedBusinessObjectName);
        }

        /// <summary>
        /// Loads the Properties of the Specified Business Object in the View.
        /// </summary>
        /// <param name="businessObject">Business Object to be Loaded.</param>
        public virtual void LoadModel(ISimpleBusinessObject businessObject)
        {
			_selectedBusinessObject = businessObject as T;

			if(_selectedBusinessObject == null || _selectedBusinessObject.Id <= 0 || CanEdit)
			{
				return;
			}

			if(SelectedTimer == null)
			{
				SelectedTimer = new Timer
				{
					Interval = 100
				};

				SelectedTimer.Elapsed += SelectedTimer_Elapsed;
			}

			SelectedTimer.Start();            
        }

		private void SelectedTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			AfterSelectBusinessObjectAction();
		}

		public virtual async Task ImportFromExcel()
        {
            try
            {
                SetViewIsBusy(ImportingFromExcelText);

                //Import Objects from Excel.
                var excelUtil = new ExcelUtil
                {
                    ViewModelBase = this
                };

                var response = await excelUtil.ImportExcel<T>();

				if(!response.Success)
				{
					return;
				}

				var tuple = response.Object;
                var businessObjects = tuple.Item1;

                foreach (var businessObject in businessObjects)
                {
                    if (!await ValidateBusinessObject(businessObject))
                    {
                        return;
                    }
                }

                var responseMessage = await Manager.InsertOrUpdateList(businessObjects);

                if (responseMessage != null && !responseMessage.Success)
                {
                    ShowFlyOutMessage(responseMessage.Message);
                    return;
                }
                else
                {
                    await SearchBusinessObjects();
                    ShowFlyOutMessage(ExcelImportedSucessfullyText);
                }
            }
            catch (Exception e)
            {
                ShowErrorMessage(e);
            }
            finally
            {
                ViewIsBusy = false;
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public virtual async Task<List<T>> GetBusinessObjectsFromDataGrid()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return new List<T>();
        }

        /// <summary>
        /// Opens the ImportDocumentsView for the type of the ViewModel.
        /// </summary>
		public virtual void OpenImportDocumentView()
		{
			var importDocumentView = new ImportDocumentsView
			{
				ExportType = typeof(T),
				CallerViewModel = this,
				MenuItem = MenuItem,
                ExcludedColumns = ExcludedColumnsInExcelImport,
                InsertObjects = InsertObjectsAfterImport
			};

			importDocumentView.CurrentDataContext.ImportFinishedEvent += ImportDocumentView_ImportFinishedEvent;

            //Get all of the ImportDocumentsView that are opened.
			var openedViews = ViewUtil.WindowsOpen(importDocumentView);

			var useInstance = false;

			foreach(var openedView in openedViews)
			{
				var openedViewAsImportView = openedView as ImportDocumentsView;

                //Get the Opened ImportDocumentsView with the same ExportType as the current Export Type.
                //This allows to have only one ImportDocumentsView for each Type.
				if(openedViewAsImportView != null)
				{
					useInstance = openedViewAsImportView.ExportType == importDocumentView.ExportType;
				}

				if (useInstance)
				{
					importDocumentView = openedViewAsImportView;
					break;
				}
			}

			InitializeView(importDocumentView, true, useInstance, true);
		}

        public override void SetTexts()
        {
            base.SetTexts();
            NewButtonTooltip = GetSystemText("NewButtonTooltip", BaseConstants.New);
            DeletingText = GetSystemText("DeletingText", BaseConstants.DeletingTextDefaultValue);
            DeleteButtonTooltip = GetSystemText("DeleteButtonTooltip", BaseConstants.DeleteButtonToolTipDefaultValue);
            RefreshButtonTooltip = GetSystemText("RefreshButtonTooltip", BaseConstants.Refresh);
            SearchButtonTooltip = GetSystemText("SearchButtonTooltip", BaseConstants.Search);
            DateText = GetSystemText("DateText", BaseConstants.Date);
            RecordsPerPageText = GetSystemText("RecordsPerPageText", BaseConstants.RecordsPerPageDefaultValue);
            OfText = GetSystemText("OfText", BaseConstants.Of.ToLower());
            GoToFirstPageText = GetSystemText("GoToFirstPageText", BaseConstants.GoToFirstPageDefaultValue);
            GoToPreviousPageText = GetSystemText("GoToPreviousPageText", BaseConstants.GoToPreviousPageDefaultValue);
            GoToNextPageText = GetSystemText("GoToNextPageText", BaseConstants.GoToNextPageDefaultValue);
            GoToLastPageText = GetSystemText("GoToLastPageText", BaseConstants.GoToLastPageDefaultValue);
            PageText = GetSystemText("PageText", BaseConstants.Page);
            CurrentPageText = GetSystemText("CurrentPageText", BaseConstants.CurrentPageDefaultValue);
            LastPageText = GetSystemText("LastPageText", BaseConstants.LastPageDefaultValue);
            RestoreDefaultValuesText = GetSystemText("RestoreDefaultValuesText", BaseConstants.RestoreDefaultValuesDefaultValue);
            ObjectDeletedSuccessfullyText = GetSystemText("ObjectDeletedSuccessfullyText", BaseConstants.ObjectDeletedSuccessfullyDefaultValue);
            SelectObjectToDeleteText = GetSystemText("SelectObjectToDeleteText", BaseConstants.SelectObjectToDeleteDefaultValue);
            ConfirmDeleteText = GetSystemText("ConfirmDeleteText", BaseConstants.ConfirmDeleteDefaultValue);
            DeleteCanceledText = GetSystemText("DeleteCanceledText", BaseConstants.DeleteCanceledDefaultValue);
            NoRecordsInPageText = GetSystemText("NoRecordsInPageText", BaseConstants.NoRecordsInPageDefaultValue);
            RelatedObjectsText = GetSystemText("RelatedObjectsText", BaseConstants.RelatedObjectsDefaultValue());
            LoadingRelatedObjectsText = GetSystemText("LoadingRelatedObjectsText", BaseConstants.LoadingRelatedObjectsDefaultValue);
            AlreadyInFirstPageText = GetSystemText("AlreadyInFirstPageText", BaseConstants.AlreadyInFirstPageDefaultValue);
            AlreadyInLastPageText = GetSystemText("AlreadyInLastPageText", BaseConstants.AlreadyInLastPageDefaultValue);
            ImportExcelButtonTooltip = GetSystemText("ImportExcelButtonTooltip", BaseConstants.ImportExcelButtonTooltipDefaultValue);
            ImportingFromExcelText = GetSystemText("ImportingFromExcelText", BaseConstants.ImportingFromExcelDefaultValue);
            ExcelImportedSucessfullyText = GetSystemText("ExcelImportedSucessfullyText", BaseConstants.ExcelImportedSucessfullyDefaultValue);
            LoadingDefaultValuesText = GetSystemText("LoadingDefaultValuesText", BaseConstants.LoadingNewObjectValuesDefaultValue);
            AdditionalText = GetSystemText("AdditionalText", BaseConstants.Additional);
            SelectBusinessObjectToEditText = GetSystemText("SelectBusinessObjectToEditText", BaseConstants.SelectBusinessObjectToEditDefaultValue);
            EditButtonToolTip = GetSystemText("EditButtonToolTip", BaseConstants.EditButtonToolTipDefaultValue);
            ViewDetailButtonToolTip = GetSystemText("ViewDetailButtonToolTip", BaseConstants.ViewDetailButtonToolTipDefaultValue);
            SearchingText = GetSystemText("SearchingText", BaseConstants.SearchingRecordsDefaultValue);
            FolioText = GetSystemText("FolioText", BaseConstants.Folio);
            CommentsText = GetSystemText("CommentsText", BaseConstants.Comments);
            TotalRecordsInDatabaseToolTip = GetSystemText("TotalRecordsInDatabaseToolTip", BaseConstants.TotalRecordsInDatabaseToolTip);
            ViewNameDoesNotEndsInViewText = GetSystemText("ViewNameDoesNotEndsInView", BaseConstants.ViewNameDoesNotEndsInViewDefaultValue());
            PopUpViewIsNotBasePopUpViewText = GetSystemText("PopUpViewIsNotBasePopUpView", BaseConstants.PopUpViewIsNotBasePopUpViewDefaultValue());
            InitializingPopUpText = GetSystemText("InitializingPopUp", BaseConstants.InitializingPopUpDefaultValue);
            LoadingNewObjectPropertiesText = GetSystemText("LoadingNewObjectProperties", BaseConstants.LoadingNewObjectPropertiesDefaultValue);
            CloningSelectedBusinessObjectText = GetSystemText("CloningSelectedBusinessObject", BaseConstants.CloningSelectedBusinessObjectDefaultValue);
            CloneSelectedBusinessObjectToolTip = GetSystemText("CloneSelectedBusinessObjectToolTip", BaseConstants.CloneSelectedBusinessObjectToolTipDefaultValue);
            CloneSelectedBusinessObjectsToolTip = GetSystemText("CloneSelectedBusinessObjectsToolTip", BaseConstants.CloneSelectedBusinessObjectsToolTipDefaultValue);
            CloningObjectsText = GetSystemText("CloningObjects", BaseConstants.CloningObjectsDefaultValue);
            CloneObjectsToolTip = GetSystemText("CloneObjectsToolTip", BaseConstants.CloneObjectsToolTipDefaultValue);
            SelectBusinessObjectToCloneText = GetSystemText("SelectBusinessObjectToClone", BaseConstants.SelectBusinessObjectToCloneDefaultValue);
            SelectBusinessObjectsToCloneText = GetSystemText("SelectBusinessObjectsToClone", BaseConstants.SelectBusinessObjectsToCloneDefaultValue);
            BusinessObjectClonedSuccessfullyText = GetSystemText("BusinessObjectClonedSuccessfully", BaseConstants.BusinessObjectClonedSuccessfullyDefaultValue);
            BusinessObjectsClonedSuccessfullyText = GetSystemText("BusinessObjectsClonedSuccessfully", BaseConstants.BusinessObjectsClonedSuccessfullyDefaultValue);
            BusinessObjectClonationCancelledText = GetSystemText("BusinessObjectClonationCancelled", BaseConstants.BusinessObjectClonationCancelledDefaultValue);

            if (SelectedBusinessObject != null)
            {
                SelectedBusinessObject.ChangeMultilanguageProperties();
            }

            AssignCancelButtonText();
            NotifyBusinessObjectMultiLanguage();
        }

        #endregion Methods
    }
}
