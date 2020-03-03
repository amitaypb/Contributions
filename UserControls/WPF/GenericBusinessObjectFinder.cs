using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects.BusinessObjects.Base;
using BusinessObjects.DTOs.Parameters;
using Base.Constants;
using Base.Enums.General;
using Framework.Utilities.Util;
using Framework.BusinessLogic.Base;
using Framework.ViewModels.Base;
using Framework.Utilities.Extensions;
using Framework.UserControls.Base;
using Base.DTOs.General;
using Base.Attributes;
using BusinessObjects.Enums.General;

namespace Framework.UserControls.BusinessObjectFinders.Base
{
    /// <summary>
    /// Generic class for Business Object Finders.
    /// </summary>
    /// <typeparam name="T">Business Object</typeparam>
    public class GenericBusinessObjectFinder<T> : BusinessObjectFinder
        where T : SimpleBusinessObject, new()
    {
        #region Properties

        private BaseBusinessObjectManager<T> _baseManager;

        protected BaseBusinessObjectManager<T> BaseManager
        {
            get
            {
                if (_baseManager == null)
                {
                    _baseManager = new BaseBusinessObjectManager<T>(ViewName);
                }

                return _baseManager;
            }
            set { _baseManager = value; }
        }

        #region Dependency Properties

        /// <summary>
        /// List of Selected Business Objects
        /// </summary>
        public static readonly DependencyProperty SelectedBusinessObjectsProperty
            = DependencyProperty.Register("SelectedBusinessObjects",
                typeof(List<T>), typeof(GenericBusinessObjectFinder<T>), new PropertyMetadata(SelectedBusinessObjectsChangedCallback));

        /// <summary>
        /// List of Selected Business Objects
        /// </summary>
        public List<T> SelectedBusinessObjects
        {
            get { return (List<T>)GetValue(SelectedBusinessObjectsProperty); }
            set { SetValue(SelectedBusinessObjectsProperty, value); }
        }

        /// <summary>
        /// ItemsSource of the Search View. If this Property is NOT Null, the UserControl won't search
        /// the Database and will use the specified List as its Items Source.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty
            = DependencyProperty.Register("ItemsSource",
                typeof(List<T>), typeof(GenericBusinessObjectFinder<T>));

        /// <summary>
        /// ItemsSource of the Search View. If this Property is NOT Null, the UserControl won't search
        /// the Database and will use the specified List as its Items Source.
        /// </summary>
        public List<T> ItemsSource
        {
            get { return (List<T>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

		#endregion Dependency Properties

		#region Multilanguage

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public virtual async Task<string> GetBusinessObjectCreatedSuccessfullyText()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
		{
            return string.Empty;
        }

		#endregion Multilanguage

		#endregion Properties

		#region Methods

		protected override async Task LoadBusinessObject()
        {
            var loadByIdResponse = await BaseManager.LoadById(SelectedBusinessObject.Id);

            if(!ProcessResponse(loadByIdResponse))
            {
                return;
            }

            SelectedBusinessObject = loadByIdResponse.Object;
        }

        /// <summary>
        /// Returns the Business Object with a code equal to the specified as a parameter
        /// </summary>
        /// <param name="code">Code of the Business Object.</param>
        /// <returns>Business Object with the specified code.</returns>
        public virtual async Task<T> GetByCode(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return default(T);
            }

            var searchParameter = new SearchParameter
            {
                SearchText = code,
                ExcludeObjectsWithNameInOtherLanguage = false
            };

            var businessObjects = await BaseManager.Search(searchParameter);
            T businessObject = await GetByCode(code, businessObjects);            

            return businessObject;
        }

        /// <summary>
        /// Returns the Business Object with a code equal to the specified as a parameter
        /// </summary>
        /// <param name="code">Code of the Business Object.</param>
        /// <returns>Business Object with the specified code.</returns>
        private async Task<T> GetByCode(string code, List<T> businessObjects)
        {
            await DelayTask();

            T businessObject = null;

            //If we are displaying the Name, then check if there is a Business Object with the specified Name.
            //If no Business Object is found with the specified Name, then check if there is a Business Object with the specified Code.
            if (DisplayMemberPath == BaseConstants.Name)
            {
                businessObject = GetBusinessObjectByName(code, businessObjects);

                if (businessObject == null)
                {
                    businessObject = GetBusinessObjectByCode(code, businessObjects);
                }
            }
            //By default, first check if there is a Business Object with the specified Code, and if no one is found, then check if there
            //is a Business Object with the specified Name.
            else
            {
                businessObject = GetBusinessObjectByCode(code, businessObjects);

                if (businessObject == null)
                {
                    businessObject = GetBusinessObjectByName(code, businessObjects);
                }
            }

            return businessObject;
        }

        private T GetBusinessObjectByCode(string code, List<T> businessObjects)
        {
            var lowerCode = code.ToLower();

            var businessObject = businessObjects.FirstOrDefault(x => x.Code.ToLower() == lowerCode);

            if (businessObject != null)
            {
                return businessObject;
            }

            //If there is no Business Object with the specified Code, get the First Business Object whose Code starts with the specified Code.
            businessObject = businessObjects.FirstOrDefault(x => x.Code.ToLower().StartsWith(lowerCode));

            if(businessObject != null)
            {
                return businessObject;
            }

            //If there is no Business Object with the specified Code, and also, there is no Business Object whose Code starts with the specified Code,
            //then get the First Business Object whose Code starts with the specified Code.
            businessObject = businessObjects.FirstOrDefault(x => x.Code.ToLower().Contains(lowerCode));

            return businessObject;
        }

        private T GetBusinessObjectByName(string code, List<T> businessObjects)
        {
            var lowerCode = code.ToLower();

            var businessObject = businessObjects.FirstOrDefault(x => x.Name.ToLower() == lowerCode);

            if (businessObject != null)
            {
                return businessObject;
            }

            //If there is no Business Object with the specified Name, try searching a Business Object
            //whose Name starts with the specified Text.
            businessObject = businessObjects.FirstOrDefault(x => x.Name.ToLower().StartsWith(lowerCode));

            if(businessObject != null)
            {
                return businessObject;
            }

            //If there is no Business Object with the specified Name, and also, there is no Business Object whose Name Starts With the specified Text,
            //try searching a Business Object whose Name contains the specified Text.
            businessObject = businessObjects.FirstOrDefault(x => x.Name.ToLower().Contains(lowerCode));

            //If there is no Business Object with the specified Name or containing the Name), try searching a Business Object
            //whose NameLanguages contains the specified Code.
            if (businessObject == null)
            {
                businessObject = businessObjects.FirstOrDefault(x => x.NameLanguages.ToLower().Contains(lowerCode));
            }

            return businessObject;
        }

        /// <summary>
        /// Event executed when the Textbox where the codes are written,
        /// loses focus. Validates that the written code (s) is/are valid (s).
        /// </summary>
        protected override async void LostFocusValidateCatalogCode()
        {
            //In ReadOnlyMode we don't validate the Code, because the User can't change it.
            if (string.IsNullOrWhiteSpace(SelectedText) || !ValidateBusinessObjectCode
			 || SelectedText.Length > TextSize + 1 || ReadOnlyMode)
            {
                return;
            }

            await CheckCodeExists(SelectedText);
        }

        /// <summary>
        /// Checks if there is a Business Object with the specified code as a parameter 
        /// </summary>
        /// <param name="code">Code of the Business Object</param>
        private async Task CheckCodeExists(string code)
        {
			try
			{
                var message = string.Format(ValidatingSpecifiedTextText, code);

                SetViewIsBusy(message);

                T businessObjectWithCode = null;

                if (typeof(T) == typeof(BusinessObject))
                {
                    if(ItemsSource == null)
                    {
                        return;
                    }
                    else
                    {
                        businessObjectWithCode = await GetByCode(code, ItemsSource);

                        if(businessObjectWithCode == null)
                        {
                            message = await GetText("CodeNotExists", "", GetType().FullName, code);

                            if (string.IsNullOrEmpty(message))
                            {
                                message = await GetText("CodeNotExists", BaseConstants.CodeNotExistsDefaultValue, ClassName, code);
                            }

                            ShowInformationMessage(message);
                            Clear();
                        }
                        else
                        {
                            SelectedBusinessObject = businessObjectWithCode;
                        }

                        return;
                    }
                }

				if (string.IsNullOrEmpty(code)
					|| (SelectedBusinessObject != null && (SelectedBusinessObject.Code == code || SelectedBusinessObject.Name == code)
					&& SelectedBusinessObject.Id > 0))
				{
					return;
				}

				//A Business Object's Code must be unique.
				businessObjectWithCode = await GetByCode(code);

				if (businessObjectWithCode != null)
				{
					if (ViewMode == ViewModeEnum.New)
					{
						message = await GetText("CantAddExistingCode", BaseConstants.CantAddExistingCodeDefaultValue, ClassName, code);
						ShowInformationMessage(message);
						SelectedText = string.Empty;
						return;
					}

					if (SelectedBusinessObjects == null)
					{
						SelectedBusinessObjects = new List<T>
					    {
						    businessObjectWithCode
					    };
					}
					else if (!SelectedBusinessObjects.Contains(businessObjectWithCode))
					{
						SelectedBusinessObjects = new List<T>(SelectedBusinessObjects)
						{
							businessObjectWithCode
						};
					}

					SelectedBusinessObject = businessObjectWithCode;
					NotifyOfPropertyChange(() => SelectedBusinessObjects);
					return;
				}

				//At this point, the businessObjectWithCode variable is NULL.
				if (ViewMode == ViewModeEnum.New)
				{
					return;
				}

				if (DatagridSelectionMode == DataGridSelectionMode.Single)
				{
					//If the Code does not exists, and the Control Can not Create New Records,
					//Display Message and Return.
					if (!CanCreate)
					{
						//Get the Name of the Object translated.
						//var objectName = await GetText(typeof(T).Name, "", typeof(T).FullName);

						message = await GetText("CodeNotExists", "", GetType().FullName, code);

						if (string.IsNullOrEmpty(message))
						{
							message = await GetText("CodeNotExists", BaseConstants.CodeNotExistsDefaultValue, 
								ClassName, code);
						}

						ShowInformationMessage(message);
						Clear();
						return;
					}

					message = await GetText("CodeNotExistsCreateNew", "", GetType().FullName, code);

					if (string.IsNullOrEmpty(message))
					{
						message = await GetText("CodeNotExistsCreateNew", BaseConstants.CodeNotExistsCreateNewDefaultValue, ClassName, code);
					}

					if (Confirm(message) != MessageBoxGeneralResult.Yes)
					{
						Clear();
						return;
					}
				}
				else if(DatagridSelectionMode == DataGridSelectionMode.Extended)
				{
					return;
				}

                await InsertNewBusinessObjectWithCode(code);
			}

			catch (Exception e)
			{
				ShowErrorMessage(e);
			}
            finally
            {
                SetViewIsBusy(false);
            }
        }

        private async Task InsertNewBusinessObjectWithCode(string code)
        {
            var newObject = new T
            {
                Code = code,
                Name = code
            };

            var response = await BaseManager.Insert(newObject);

            if (response.Success)
            {
                SelectedBusinessObject = newObject;

                var insertSucessfullMessage = await GetBusinessObjectCreatedSuccessfullyText();

                if (!string.IsNullOrEmpty(insertSucessfullMessage))
                {
                    response.Message = insertSucessfullMessage;
                }
            }

            ShowFlyOutMessage(response.Message);
        }

        public override void AssignSelectedBusinessObjects(BaseViewModel baseViewModel)
        {
            if(baseViewModel == null)
            {
                SelectedBusinessObjects = null;
                return;
            }

            base.AssignSelectedBusinessObjects(baseViewModel);

            var baseSearchViewModel = baseViewModel as BaseSearchViewModel<T>;

            if (baseSearchViewModel.SelectedBusinessObjects != null && DatagridSelectionMode == DataGridSelectionMode.Extended)
            {
                SelectedBusinessObjects = baseSearchViewModel.SelectedBusinessObjects;

                var codes = string.Empty;

                foreach (var businessObject in SelectedBusinessObjects)
                {
                    if (!string.IsNullOrEmpty(codes))
                    {
                        codes += ", ";
                    }

                    codes += businessObject.Code;
                }

				SelectedText = codes;
				NotifyOfPropertyChange(() => SelectedText);
            }
            else
            {
                SelectedBusinessObjects = null;
            }
        }

        public override void AssignItemsSource(BaseViewModel baseViewModel)
        {
            if (baseViewModel == null)
            {
                SelectedBusinessObjects = null;
                return;
            }

            base.AssignItemsSource(baseViewModel);

            var baseSearchViewModel = baseViewModel as BaseSearchViewModel<T>;

            baseSearchViewModel.ItemsSource = ItemsSource;

            if(typeof(T) == typeof(BusinessObject) && ItemsSource == null)
            {
                baseSearchViewModel.ItemsSource = new List<T>();
            }
        }

        public override void SetCanCreate()
        {
            base.SetCanCreate();

			//If the Type of the Object is an Enum, hide the Open with Option and set the CanCreate = false, 
			//because Enums does not have a View, and they can not ba created.
			if (ReflectionUtil.TypeHaveBaseType(typeof(T), typeof(BusinessObjectEnum<>)))
			{
				OpenWithMenuVisibility = Visibility.Collapsed;
				CanCreate = false;
			}
			else
			{
				//By Default, we allow to Create only the Business Objects that does not contains
				//non Optional Business Objects.
				var properties = ReflectionUtil.GetPropertiesInfo(typeof(T));

				foreach (var property in properties)
				{
					if (property.Name != BaseConstants.CreatedBy
					 && property.Name != BaseConstants.DeletedBy
					 && property.Name != BaseConstants.UpdatedBy
					 && property.Name != BaseConstants.Company
					 && ReflectionUtil.TypeHaveBaseType(property.PropertyType, BaseConstants.BaseBusinessObject)
					 && !ReflectionUtil.HaveAttribute(property, typeof(OptionalMappingAttribute)))
					{
						CanCreate = false;
                        return;
					}
				}

                CanCreate = true;
			}
        }

        /// <summary>
        /// Callback executed when the Selected Business Objects changes.
        /// </summary>
        /// <param name="sender">Object sender of the event</param>
        /// <param name="e">Event parameters</param>
        private static void SelectedBusinessObjectsChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as GenericBusinessObjectFinder<T>;

            if (control == null)
            {
                return;
            }

            if (control.SelectedBusinessObjects != null)
            {
                var codes = string.Empty;

                foreach (var businessObject in control.SelectedBusinessObjects)
                {
                    if (!string.IsNullOrEmpty(codes))
                    {
                        codes += ", ";
                    }

                    codes += businessObject.Code;
                }

                control.SelectedText = codes;
				control.NotifyOfPropertyChange(() => control.SelectedText);
            }
            else if(control.SelectedBusinessObjects == null && control.SelectedBusinessObject == null)
            {
                control.Clear();
            }
        }

		/// <summary>
		/// Searches the Texts for the Current UserControl.
		/// </summary>
		public override async Task<List<SystemTextDTO>> SearchTexts(string typeName = "", bool searchBaseTexts = true)
		{
			//1- Search Texts of the BusinessObjectFinder UserControl.
			var searchParameter = new SearchParameter
			{
				SearchText = typeof(BusinessObjectFinder).FullName
			};

			List<SystemTextDTO> systemTexts = await SystemTextManager.SearchDTOs(searchParameter);

			//2- Search Texts of BaseUserControl.
			var baseSystemTexts = await SystemTextManager.SearchByLanguage(typeof(BaseUserControl).FullName);

			systemTexts.AddRange(baseSystemTexts);

			//3- Search Texts of Current UserControl.
			searchParameter.SearchText = GetType().FullName;
			var currentTexts = await SystemTextManager.SearchDTOs(searchParameter);

			systemTexts.AddRange(currentTexts);

			//4- Search Texts of the Object of the User Control.
			searchParameter.SearchText = typeof(T).FullName;
			var objectTexts = await SystemTextManager.SearchDTOs(searchParameter);

			systemTexts.AddRange(objectTexts);

			SystemTexts = systemTexts;

            return SystemTexts;
		}

		public override void SetTexts()
		{
			base.SetTexts();

			if (!TakeTitleFromView)
			{
				var title = GetSystemText(typeof(T).Name);

                //Remove articles from the title to don't grow the Button too much.
                if (ButtonTextRemoveArticles)
                {
                    Title = title.RemoveArticles();
                }
                else
                {
                    Title = title;
                }
			}

			if (!TakeToolTipFromView)
			{
                ToolTip = GetSystemText(BaseConstants.ButtonToolTip, string.Empty, null, false);

                if (ToolTip == null || string.IsNullOrEmpty(ToolTip.ToString()))
                {
                    ToolTip = GetSystemText(typeof(T).Name);
                }
			}

			SendInitializationEndedEvent(this, new EventArgs());
        }

		public override async void OpenSearchView()
		{
			await OpenSearchViewAsync();
		}

		/// <summary>
		/// Shows the Search View associated to this control
		/// </summary>
		public virtual async Task OpenSearchViewAsync()
		{
            //On ReadOnly Mode do not Open the Search View, because the Selected Business Object can not be changed.
            if (ReadOnlyMode && !SearchInReadOnlyMode)
            {
                return;
            }

			var response = await ViewModelUtil.GetSearchView<T>();

			if(!response.Success)
			{
				ShowFlyOutMessage(response.Message);
				return;
			}

			var searchView = response.Object as dynamic;

			if (!string.IsNullOrEmpty(ExcludedBusinessObjectsText))
			{
				searchView.DataContext.SelectedBusinessObjectExcludedText = ExcludedBusinessObjectsText;
			}

			searchView.DataContext.DataGridSelectionMode = DatagridSelectionMode;
			searchView.DataContext.ExcludedIds = ExcludedIds;
			searchView.DataContext.BusinessObjectToSearch = BusinessObjectToSearch;
            searchView.DataContext.SkipSetViewTitle = !string.IsNullOrEmpty(ViewTitle);
            searchView.DataContext.ViewTitle = ViewTitle;

			AssignItemsSource(searchView.DataContext);
			searchView.ShowDialog();

			//If no Business Object was Selected, return.
			if ((searchView.DataContext.SelectedBusinessObject == null && searchView.DataContext.SelectedBusinessObjects == null)
			|| !searchView.DataContext.OkButtonClicked)
			{
				return;
			}

			SelectedBusinessObject = searchView.DataContext.SelectedBusinessObject;

			AssignSelectedBusinessObjects(searchView.DataContext);
		}

		#endregion Methods
	}
}
