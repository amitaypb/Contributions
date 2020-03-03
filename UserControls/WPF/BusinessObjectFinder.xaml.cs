using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BusinessObjects.BusinessObjects.Base;
using BusinessObjects.Interfaces;
using BusinessObjects.Util;
using Base.Constants;
using GalaSoft.MvvmLight.Command;
using Framework.Commands;
using Framework.Utilities.Util;
using Framework.ViewModels.Base;
using Base.Util;
using Framework.UserControls.Base;
using System.Drawing;
using Base.Attributes;
using BusinessObjects.Enums.General;

namespace Framework.UserControls.BusinessObjectFinders.Base
{
    /// <summary>
    /// Interaction logic for BusinessObjectFinder.xaml
    /// </summary>
    public partial class BusinessObjectFinder : BaseUserControl
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public BusinessObjectFinder()
        {
            InitializeComponent();
            ValidateBusinessObjectCode = true;
            LostFocusCommand = new RelayCommand(LostFocusValidateCatalogCode);
            OpenSearchViewCommand = new RelayCommand(OpenSearchView);
            OpenViewCommand = new RelayCommand(OpenView);
            OpenViewAsyncCommand = AsyncCommand.Create(OpenViewAsync);
			ChangeDisplayMemberPathCommand = new RelayCommand(ChangeDisplayMemberPath);

			SearchViewName = BaseConstants.BusinessObjectsSearchViewName;

            //ButtonImg.InitializationEndedEvent += SendInitializationEndedEvent;
		}

        #endregion Constructor

        #region Properties

        protected override string ClassName
        {
            get { return typeof(BusinessObjectFinder).FullName; }
        }

        private bool _canCreate;
        protected bool CanCreate
        {
            get { return _canCreate; }
            set
            {
                _canCreate = value;
                NotifyOfPropertyChange(() => CanCreate);
            }
        }

		//protected string _selectedText;
		//public string SelectedText
		//{
		//	get { return _selectedText; }
		//	set
		//	{
		//		_selectedText = value;
		//		NotifyOfPropertyChange(() => SelectedText);

		//		TextChanged();
		//	}
		//}

		/// <summary>
		/// Path of the Icon for the Cut Context Menu 
		/// </summary>
		public System.Windows.Controls.Image CutIcon
		{
			get
			{
				return new System.Windows.Controls.Image
				{
					Source = IconUtil.GetMenuIconResourceFullPath(IconUtil.CutIconName)
				};
			}
		}

		/// <summary>
		/// Path of the Icon for the Copy Context Menu 
		/// </summary>
		public System.Windows.Controls.Image CopyIcon
		{
			get
			{
				return new System.Windows.Controls.Image
				{
					Source = IconUtil.GetMenuIconResourceFullPath(IconUtil.CopyIconName)
				};
			}
		}

		/// <summary>
		/// Path of the Icon for the Paste Context Menu 
		/// </summary>
		public System.Windows.Controls.Image PasteIcon
		{
			get
			{
				return new System.Windows.Controls.Image
				{
					Source = IconUtil.GetMenuIconResourceFullPath(IconUtil.PasteIconName)
				};
			}
		}

		public double ButtonActualWidth
        {
            get { return ButtonImg.ActualWidth; }
        }

		private List<PropertyInfo> _businessObjectProperties;
		public List<PropertyInfo> BusinessObjectProperties
		{
			get
			{
				if(_businessObjectProperties == null)
				{
					_businessObjectProperties = ReflectionUtil.GetPropertiesInfo(SelectedBusinessObject.GetType());
				}

				return _businessObjectProperties;
			}
		}

		/// <summary>
		/// Tab index of the button
		/// </summary>
		public int ButtonTabIndex
		{
			get { return TabIndex + 1; }
		}

		private string _errorMessage;
		/// <summary>
		/// Error message to be displayed to the user
		/// </summary>
		public string ErrorMessageToUser
		{
			get { return _errorMessage; }
			set
			{
				_errorMessage = value;
				NotifyOfPropertyChange(() => ErrorMessageToUser);
			}
		}

		public string SelectedBusinessObjectName
		{
			get
			{
				return SelectedBusinessObject == null
				  ? string.Empty : SelectedBusinessObject.Name;
			}
		}

		public BaseBusinessObject BusinessObjectToSearch { get; set; }

		public string SearchViewDataContextName { get; set; }

		/// <summary>
		/// Path of the Icon for the Open With Context Menu 
		/// </summary>
		public virtual System.Windows.Controls.Image OpenWithIcon
		{
			get
			{
				return new System.Windows.Controls.Image
				{
					Source = IconUtil.GetMenuIconResourceFullPath(IconName)
				};
			}
		}

		/// <summary>
		/// Path of the Icon for the Show Code Context Menu 
		/// </summary>
		public virtual System.Windows.Controls.Image ShowCodeIcon
		{
			get
			{
				return new System.Windows.Controls.Image
				{
					Source = IconUtil.GetMenuIconResourceFullPath(IconUtil.BinaryIconName)
				};
			}
		}

		/// <summary>
		/// Path of the Icon for the Show Name Context Menu 
		/// </summary>
		public virtual System.Windows.Controls.Image ShowNameIcon
		{
			get
			{
				return new System.Windows.Controls.Image
				{
					Source = IconUtil.GetMenuIconResourceFullPath(IconUtil.DocumentWriteIconName)
				};
			}
		}

		public virtual System.Windows.Controls.Image ShowCodeOrNameIcon
		{
			get
			{
				if(DisplayMemberPath == BaseConstants.Name)
				{
					return ShowCodeIcon;
				}

				return ShowNameIcon;
			}
		}

        #region Dependency Properties

        /// <summary>
        /// Indicates the Group of the Business Object Finders.
        /// Similar to Radio Buttons, Business Object Finders with the same Group, will have the same Button's Width.
        /// </summary>
        public static readonly DependencyProperty GroupProperty
            = DependencyProperty.Register("Group", typeof(int), typeof(BusinessObjectFinder));

        /// <summary>
        /// Indicates the Group of the Business Object Finders.
        /// Similar to Radio Buttons, Business Object Finders with the same Group, will have the same Button's Width.
        /// </summary>
        public int Group
        {
            get { return (int)GetValue(GroupProperty); }
            set { SetValue(GroupProperty, value); }
        }

        /// <summary>
        /// Text of the TextBox.
        /// </summary>
        public static readonly DependencyProperty SelectedTextProperty
            = DependencyProperty.Register("SelectedText", typeof(string), typeof(BusinessObjectFinder),
                new PropertyMetadata(SelectedTextChangedCallback));

        /// <summary>
        /// Text of the TextBox.
        /// </summary>
        public string SelectedText
        {
            get { return (string)GetValue(SelectedTextProperty); }
            set { SetValue(SelectedTextProperty, value); }
        }

        /// <summary>
		/// Name of the Icon.
		/// </summary>
		public static readonly DependencyProperty IconNameProperty
            = DependencyProperty.Register("IconName", typeof(string), typeof(BusinessObjectFinder));

        /// <summary>
        /// Name of the Icon of the Button.
        /// </summary>
        public string IconName
        {
            get { return (string)GetValue(IconNameProperty); }
            set { SetValue(IconNameProperty, value); }
        }

        /// <summary>
        /// Indicates if the Control will validate the Code written by the User in the textbox.
        /// </summary>
        public static readonly DependencyProperty ValidateBusinessObjectCodeProperty
            = DependencyProperty.Register("ValidateBusinessObjectCode", typeof(bool), typeof(BusinessObjectFinder));

        /// <summary>
        /// Indicates if the Control will validate the Codes written by the User in the textbox.
        /// </summary>
        public bool ValidateBusinessObjectCode
        {
            get { return (bool)GetValue(ValidateBusinessObjectCodeProperty); }
            set { SetValue(ValidateBusinessObjectCodeProperty, value); }
        }

        /// <summary>
        /// Id of the Selected Business Object
        /// </summary>
        public static readonly DependencyProperty SelectedIdProperty
            = DependencyProperty.Register("SelectedId", typeof(string), typeof(BusinessObjectFinder));

        /// <summary>
        /// Id of the Selected Business Object
        /// </summary>
        public string SelectedId
        {
            get { return (string)GetValue(SelectedIdProperty); }
            set { SetValue(SelectedIdProperty, value); }
        }

        /// <summary>
        /// Property of the Title of the Control.
        /// Should be the Name of the Business Object.
        /// </summary>
        public static readonly DependencyProperty TitleProperty
            = DependencyProperty.Register("Title", typeof(string), typeof(BusinessObjectFinder));

        /// <summary>
        /// Title of the Control.
        /// Should be the Name of the Business Object
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty LabelFontSizeProperty =
            DependencyProperty.Register("LabelFontSize", typeof(double), typeof(BusinessObjectFinder));

        /// <summary>
        /// Font size of the Label
        /// </summary>
        public double LabelFontSize
        {
            get { return (double)GetValue(LabelFontSizeProperty); }
            set { SetValue(LabelFontSizeProperty, value); }
        }

        public static readonly DependencyProperty LabelFontWeightProperty =
            DependencyProperty.Register("LabelFontWeight", typeof(FontWeight), typeof(BusinessObjectFinder));

        /// <summary>
        /// Font weight of the Label
        /// </summary>
        public FontWeight LabelFontWeight
        {
            get { return (FontWeight)GetValue(LabelFontWeightProperty); }
            set { SetValue(LabelFontWeightProperty, value); }
        }

        public static readonly DependencyProperty LabelFontFamilyProperty =
            DependencyProperty.Register("LabelFontFamily", typeof(System.Windows.Media.FontFamily), typeof(BusinessObjectFinder));

        public System.Windows.Media.FontFamily LabelFontFamily
        {
            get { return (System.Windows.Media.FontFamily)GetValue(LabelFontFamilyProperty); }
            set { SetValue(LabelFontFamilyProperty, value); }
        }

        /// <summary>
        /// List with the excluded Ids
        /// </summary>
        public static readonly DependencyProperty ExcludedIdsProperty
            = DependencyProperty.Register("ExcludedIds", typeof(List<long>), typeof(BusinessObjectFinder));

        /// <summary>
        /// List with the Ids to be excluded in the Search
        /// </summary>
        public List<long> ExcludedIds
        {
            get { return (List<long>)GetValue(ExcludedIdsProperty); }
            set { SetValue(ExcludedIdsProperty, value); }
        }

        /// <summary>
        /// Selected Business Object
        /// </summary>
        public static readonly DependencyProperty SelectedBusinessObjectProperty
            = DependencyProperty.Register("SelectedBusinessObject", typeof(ISimpleBusinessObject),
                typeof(BusinessObjectFinder), new PropertyMetadata(SelectedModelChangedCallback));

        /// <summary>
        /// Selected Business Object selected by the User in the Search.
        /// </summary>
        public ISimpleBusinessObject SelectedBusinessObject
        {
            get { return (ISimpleBusinessObject)GetValue(SelectedBusinessObjectProperty); }
            set { SetValue(SelectedBusinessObjectProperty, value); }
        }

        /// <summary>
        /// Assembly Name of the Search View to open
        /// </summary>
        public static readonly DependencyProperty AssemblyNameProperty
            = DependencyProperty.Register("AssemblyName", typeof(string), typeof(BusinessObjectFinder));

        /// <summary>
        /// Name of the Assembly of the Search View to Open.
        /// </summary>
        public string AssemblyName
        {
            get { return (string)GetValue(AssemblyNameProperty); }
            set { SetValue(AssemblyNameProperty, value); }
        }

        /// <summary>
        /// Name of the Search View to open
        /// </summary>
        public static readonly DependencyProperty SearchViewNameProperty
            = DependencyProperty.Register("SearchViewName", typeof(string), typeof(BusinessObjectFinder));

        /// <summary>
        /// Name of the Search View to Open.
        /// </summary>
        public string SearchViewName
        {
            get { return (string)GetValue(SearchViewNameProperty); }
            set { SetValue(SearchViewNameProperty, value); }
        }

        /// <summary>
        /// Selection Mode in the Datagrid.
        /// By Default, it will be Single Selection.
        /// </summary>
        public static readonly DependencyProperty DatagridSelectionModeProperty
            = DependencyProperty.Register("DatagridSelectionMode", typeof(DataGridSelectionMode)
                , typeof(BusinessObjectFinder), new UIPropertyMetadata(DataGridSelectionMode.Single));

        /// <summary>
        /// Selection Mode of the Datagrid.
        /// </summary>
        public DataGridSelectionMode DatagridSelectionMode
        {
            get { return (DataGridSelectionMode)GetValue(DatagridSelectionModeProperty); }
            set { SetValue(DatagridSelectionModeProperty, value); }
        }

        /// <summary>
        /// Width of the Textbox
        /// </summary>
        public static readonly DependencyProperty TextboxWidthProperty
            = DependencyProperty.Register("TextboxWidth", typeof(double), typeof(BusinessObjectFinder),
				new UIPropertyMetadata(double.NaN));

        /// <summary>
        /// Width of the Textbox
        /// </summary>
        public double TextboxWidth
        {
            get { return (double)GetValue(TextboxWidthProperty); }
            set { SetValue(TextboxWidthProperty, value); }
        }

        /// <summary>
        /// ViewMode State
        /// </summary>
        public static readonly DependencyProperty ViewModeProperty
            = DependencyProperty.Register("ViewMode", typeof(ViewModeEnum), typeof(BusinessObjectFinder));

        /// <summary>
        /// ViewMode
        /// </summary>
        public ViewModeEnum ViewMode
        {
            get { return (ViewModeEnum)GetValue(ViewModeProperty); }
            set { SetValue(ViewModeProperty, value); }
        }

        /// <summary>
        /// Focus/Unfocus the Textbox
        /// </summary>
        public static readonly DependencyProperty TextboxIsFocusedProperty
            = DependencyProperty.Register("TextboxIsFocused", typeof(bool), typeof(BusinessObjectFinder));

        /// <summary>
        /// Focus/Unfocus the Textbox
        /// </summary>
        public bool TextboxIsFocused
        {
            get { return (bool)GetValue(TextboxIsFocusedProperty); }
            set { SetValue(TextboxIsFocusedProperty, value); }
        }

        /// <summary>
        /// Indicates if the Control is in ReadOnly Mode
        /// </summary>
        public static readonly DependencyProperty ReadOnlyModeProperty
            = DependencyProperty.Register("ReadOnlyMode", typeof(bool), typeof(BusinessObjectFinder),
                new PropertyMetadata(ReadOnlyModeChangedCallback));

        /// <summary>
        /// Indicates if the Control is in ReadOnly Mode.
        /// </summary>
        public bool ReadOnlyMode
        {
            get { return (bool)GetValue(ReadOnlyModeProperty); }
            set { SetValue(ReadOnlyModeProperty, value); }
        }

        /// <summary>
        /// Indicates if the TextBox Accepts Text
        /// </summary>
        public static readonly DependencyProperty AcceptsTextProperty
            = DependencyProperty.Register("AcceptsText", typeof(bool), typeof(BusinessObjectFinder),
            new UIPropertyMetadata(true));

        /// <summary>
        /// Indicates if the TextBox Accepts Text
        /// </summary>
        public bool AcceptsText
        {
            get { return (bool)GetValue(AcceptsTextProperty); }
            set { SetValue(AcceptsTextProperty, value); }
        }

        /// <summary>
        /// Visibility of the Open With Menu.
        /// </summary>
        public static readonly DependencyProperty OpenWithMenuVisibilityProperty
            = DependencyProperty.Register("OpenWithMenuVisibility", typeof(Visibility), typeof(BusinessObjectFinder),
            new UIPropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Visibility of the Open With Menu.
        /// </summary>
        public Visibility OpenWithMenuVisibility
        {
            get { return (Visibility)GetValue(OpenWithMenuVisibilityProperty); }
            set { SetValue(OpenWithMenuVisibilityProperty, value); }
        }

        [Category("UserControl")]
        public ImageSource ButtonImage
        {
            get { return (ImageSource)GetValue(ButtonImageProperty); }
            set { SetValue(ButtonImageProperty, value); }
        }

        public static readonly DependencyProperty ButtonImageProperty =
            DependencyProperty.Register("ButtonImage", typeof(ImageSource), typeof(BusinessObjectFinder), new UIPropertyMetadata(null));

        /// <summary>
        /// Size of the Text in the TextBox.
        /// </summary>
        public int TextSize
        {
            get { return (int)GetValue(TextSizeProperty); }
            set { SetValue(TextSizeProperty, value); }
        }

        public static readonly DependencyProperty TextSizeProperty =
            DependencyProperty.Register("TextSize", typeof(int), typeof(BusinessObjectFinder),
                new UIPropertyMetadata(60));

        /// <summary>
        /// Background of the TextBox.
        /// </summary>
        public System.Windows.Media.Brush TextBoxBackground
        {
            get { return (System.Windows.Media.Brush)GetValue(TextBoxBackgroundProperty); }
            set { SetValue(TextBoxBackgroundProperty, value); }
        }

        public static readonly DependencyProperty TextBoxBackgroundProperty =
            DependencyProperty.Register("TextBoxBackground", typeof(System.Windows.Media.Brush), typeof(BusinessObjectFinder),
                new UIPropertyMetadata(System.Windows.Media.Brushes.White));

        /// <summary>
        /// ClearTextButton of the TextBox.
        /// </summary>
        public bool ClearTextButton
        {
            get { return (bool)GetValue(ClearTextButtonProperty); }
            set { SetValue(ClearTextButtonProperty, value); }
        }

        public static readonly DependencyProperty ClearTextButtonProperty =
            DependencyProperty.Register("ClearTextButton", typeof(bool), typeof(BusinessObjectFinder),
                new UIPropertyMetadata(true));

        /// <summary>
        /// Indicates if the TextBox is ReadOly.
        /// </summary>
        public bool TextBoxReadOnly
        {
            get { return (bool)GetValue(TextBoxReadOnlyProperty); }
            set { SetValue(TextBoxReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty TextBoxReadOnlyProperty =
            DependencyProperty.Register("TextBoxReadOnly", typeof(bool), typeof(BusinessObjectFinder),
                new UIPropertyMetadata(false, TextBoxReadOnlyChangedCallback));

		/// <summary>
		/// Name of the Property to be displayed on the TextBox.
		/// </summary>
		public string DisplayMemberPath
		{
			get { return (string)GetValue(DisplayMemberPathProperty); }
			set { SetValue(DisplayMemberPathProperty, value); }
		}

		public static readonly DependencyProperty DisplayMemberPathProperty =
			DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(BusinessObjectFinder),
				new UIPropertyMetadata(BaseConstants.Name));

		/// <summary>
		/// HorizontalAlignment of the TextBox.
		/// </summary>
		public HorizontalAlignment TextBoxHorizontalAlignment
		{
			get { return (HorizontalAlignment)GetValue(TextBoxHorizontalAlignmentProperty); }
			set { SetValue(TextBoxHorizontalAlignmentProperty, value); }
		}

		public static readonly DependencyProperty TextBoxHorizontalAlignmentProperty =
			DependencyProperty.Register("TextBoxHorizontalAlignment", typeof(HorizontalAlignment), typeof(BusinessObjectFinder),
				new UIPropertyMetadata(HorizontalAlignment.Stretch));

        /// <summary>
        /// Indicates if the Title (Text of the TextBox) will be set in the View that Uses the UserControl.
        /// </summary>
        public bool TakeTitleFromView
        {
            get { return (bool)GetValue(TakeTitleFromViewProperty); }
            set { SetValue(TakeTitleFromViewProperty, value); }
        }

        public static readonly DependencyProperty TakeTitleFromViewProperty =
            DependencyProperty.Register("TakeTitleFromView", typeof(bool), typeof(BusinessObjectFinder));

        /// <summary>
        /// Indicates if the ToolTip will be set in the View that Uses the UserControl.
        /// </summary>
        public bool TakeToolTipFromView
        {
            get { return (bool)GetValue(TakeToolTipFromViewProperty); }
            set { SetValue(TakeToolTipFromViewProperty, value); }
        }

        public static readonly DependencyProperty TakeToolTipFromViewProperty =
            DependencyProperty.Register("TakeToolTipFromView", typeof(bool), typeof(BusinessObjectFinder));

		/// <summary>
		/// Indicates if the ToolTip will be set in the View that Uses the UserControl.
		/// </summary>
		public string ExcludedBusinessObjectsText
		{
			get { return (string)GetValue(ExcludedBusinessObjectsTextProperty); }
			set { SetValue(ExcludedBusinessObjectsTextProperty, value); }
		}

		public static readonly DependencyProperty ExcludedBusinessObjectsTextProperty =
			DependencyProperty.Register("ExcludedBusinessObjectsText", typeof(string), typeof(BusinessObjectFinder));

        /// <summary>
		/// Indicates if the Button will open the Searh View even in ReadOnlyMode.
        /// This is the case, when you want to force the User to have a Selected Business Object (Not Null)
        /// but you want to change it.
        /// For instance: MenuView.
		/// </summary>
		public bool SearchInReadOnlyMode
        {
            get { return (bool)GetValue(SearchInReadOnlyModeProperty); }
            set { SetValue(SearchInReadOnlyModeProperty, value); }
        }

        public static readonly DependencyProperty SearchInReadOnlyModeProperty =
            DependencyProperty.Register("SearchInReadOnlyMode", typeof(bool), typeof(BusinessObjectFinder));

        /// <summary>
		/// Indicates if the Articles will be removed from the Text of the Button.
		/// </summary>
		public bool ButtonTextRemoveArticles
        {
            get { return (bool)GetValue(ButtonTextRemoveArticlesProperty); }
            set { SetValue(ButtonTextRemoveArticlesProperty, value); }
        }

        public static readonly DependencyProperty ButtonTextRemoveArticlesProperty =
            DependencyProperty.Register("ButtonTextRemoveArticles", typeof(bool), typeof(BusinessObjectFinder), new UIPropertyMetadata(true));

        /// <summary>
		/// Title of the Search View.
		/// </summary>
		public static readonly DependencyProperty ViewTitleProperty
            = DependencyProperty.Register("ViewTitle", typeof(string), typeof(BusinessObjectFinder));

        /// <summary>
        /// Title of the Search View.
        /// </summary>
        public string ViewTitle
        {
            get { return (string)GetValue(ViewTitleProperty); }
            set { SetValue(ViewTitleProperty, value); }
        }

        public bool SetIsFocused
        {
            get { return (bool)GetValue(SetIsFocusedProperty); }
            set { SetValue(SetIsFocusedProperty, value); }
        }

        public static readonly DependencyProperty SetIsFocusedProperty =
            DependencyProperty.Register("SetIsFocused", typeof(bool), typeof(BusinessObjectFinder),
                new UIPropertyMetadata(false));

        /// <summary>
		/// Indicates that this UserControl will always have its Auto Width.
		/// For instance: When there are more than One Business Object Finder, in the BaseView class
		/// all of the Business Object Finders are being set to have the same Width.
		/// This property prevents this. This is used in the Base Price List in the Edit Tab of the Price Lists View.
		/// </summary>
        public bool PreventSetWidth
        {
            get { return (bool)GetValue(PreventSetWidthProperty); }
            set { SetValue(PreventSetWidthProperty, value); }
        }

        public static readonly DependencyProperty PreventSetWidthProperty =
            DependencyProperty.Register("PreventSetWidth", typeof(bool), typeof(BusinessObjectFinder));

        #endregion Dependency Properties

        #region Multilanguage

        private string _openWithText;
        public string OpenWithText
        {
            get { return _openWithText; }
            set
            {
                _openWithText = value;
                NotifyOfPropertyChange(() => OpenWithText);
            }
        }

        private string _cutText;
        public string CutText
        {
            get { return _cutText; }
            set
            {
                _cutText = value;
                NotifyOfPropertyChange(() => CutText);
            }
        }

        private string _copyText;
        public string CopyText
        {
            get { return _copyText; }
            set
            {
                _copyText = value;
                NotifyOfPropertyChange(() => CopyText);
            }
        }

        private string _pasteText;
        public string PasteText
        {
            get { return _pasteText; }
            set
            {
                _pasteText = value;
                NotifyOfPropertyChange(() => PasteText);
            }
        }

		private string _showCodeText;
		public string ShowCodeText
		{
			get { return _showCodeText; }
			set
			{
				_showCodeText = value;
				NotifyOfPropertyChange(() => ShowCodeText);
			}
		}

		private string _showNameText;
		public string ShowNameText
		{
			get { return _showNameText; }
			set
			{
				_showNameText = value;
				NotifyOfPropertyChange(() => ShowNameText);
			}
		}

		public string ShowCodeOrNameText
		{
			get
			{
				if (DisplayMemberPath == BaseConstants.Name)
				{
					return ShowCodeText;
				}

				return ShowNameText;
			}
		}

        private string _validatingSpecifiedTextText;
        public string ValidatingSpecifiedTextText
        {
            get { return _validatingSpecifiedTextText; }
            set
            {
                _validatingSpecifiedTextText = value;
                NotifyOfPropertyChange(() => ValidatingSpecifiedTextText);
            }
        }

        #endregion Multilanguage

        #endregion Properties

        #region Commands

        /// <summary>
        /// Command executed when the Codes Textbox loses focus
        /// </summary>
        public ICommand LostFocusCommand { get; set; }

        /// <summary>
        /// Command executed when the Button is clicked.
        /// </summary>
        public ICommand OpenSearchViewCommand { get; set; }

        /// <summary>
        /// Command executed when the Open With Context Menu is clicked
        /// </summary>
        public ICommand OpenViewCommand { get; set; }

        /// <summary>
        /// Command executed when the Open With Context Menu is clicked
        /// </summary>
        public AsyncCommand<object> OpenViewAsyncCommand { get; set; }

        /// <summary>
        /// Command executed before the Context Menu is Open.
        /// </summary>
        public ICommand ContextMenuOpeningCommand { get; set; }

		/// <summary>
		/// Command to change the DisplayMemberPath of the BusinessObjectFinder UserControl.
		/// </summary>
		public ICommand ChangeDisplayMemberPathCommand { get; set; }

        #endregion Commands

        #region Methods

        #region Public

		public virtual double GetButtonTextWidth()
		{
            //var charWidth = 9d;

            //if(!string.IsNullOrEmpty(Title))
            //{
            //	foreach(var charTitle in Title)
            //	{
            //		if(charTitle == 'h' || charTitle == 's' || charTitle == 'é')
            //		{
            //			charWidth += 0.1;
            //		}
            //		else if (charTitle == 'W' || charTitle == 'm' || charTitle == 'M' || charTitle == 'ñ' || charTitle == 'C'
            //                       || charTitle == 'í' || charTitle == 'p' || charTitle == 'Y' || charTitle == 'y')
            //		{
            //			charWidth += 0.4;
            //		}
            //	}
            //}

            var textSize = MeasureString(Title);
            var titleWidth = textSize.Width;
            //var titleLength = string.IsNullOrEmpty(Title) ? 0 : Title.Length * 2.5;

			return ButtonImg.ImageWidth + titleWidth;
		}

		private SizeF MeasureString(string text)
		{
            SizeF size = new SizeF(0, 0);

            if (string.IsNullOrEmpty(text))
			{
                //return new System.Windows.Size(0, 0);
                return size;
			}

            //var formattedText = new FormattedText(
            //	text,
            //	CultureInfo.CurrentCulture,
            //	FlowDirection.LeftToRight,
            //	new Typeface(new FontFamily("Candara"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
            //	22,
            //	Brushes.Black,
            //	new NumberSubstitution());

            using (Graphics graphics = Graphics.FromImage(new Bitmap(1, 1)))
            {
                size = graphics.MeasureString(text, new Font("Cambria Math", 13, System.Drawing.FontStyle.Regular, GraphicsUnit.Point));
            }

            //return new Size(formattedText.Width, formattedText.Height);
            return size;
		}

		public double GetButtonActualWidth()
        {
            return ButtonImg.ActualWidth;
        }

        /// <summary>
		/// Sets the Width of the Button.
		/// </summary>
		/// <param name="width">Width.</param>
		public void SetButtonWidth(double width)
        {
            ButtonImg.Width = width;
        }

        /// <summary>
        /// Clears the Control
        /// </summary>
        public void Clear()
        {
			SelectedText = string.Empty;
			SelectedId = string.Empty;
            SelectedBusinessObject = null;
            AssignSelectedBusinessObjects(null);
		}

		/// <summary>
		/// Shows the Search View associated to this control
		/// </summary>
		public virtual void OpenSearchView() { }

        public virtual void AssignSelectedBusinessObjects(BaseViewModel baseViewModel) { }

        public virtual void AssignItemsSource(BaseViewModel baseViewModel) { }

        public virtual void Initialize()
		{
			AssemblyName = BaseConstants.AssemblyName;

			if (!string.IsNullOrEmpty(IconName))
			{
				ButtonImage = IconUtil.GetMenuIconResource(IconName);
			}
		}

        public override void LoadedEventMethod()
        {
            base.LoadedEventMethod();

            //In Design Mode, do nothing.
            if (DesignerProperties.GetIsInDesignMode(DependObject))
            {
                return;
            }

            Initialize();

            if (SetIsFocused)
            {
                BusinessObjectTextBox.Focus();
            }

            SessionVariables.Instance.EditPermissionUpdatedEvent += Instance_EditPermissionUpdatedEvent;

            SetCanCreate();

            NotifyOfPropertyChange(() => ButtonActualWidth);

            ParentWindow.AddBusinessObjectFinder(this);
        }

        public virtual void SetCanCreate(){}

        public override void SetFocusToObject()
        {
            base.SetFocusToObject();
            BusinessObjectTextBox.Focus();
        }

		public override void SetTexts()
		{
			base.SetTexts();

			CutText = GetSystemText("CutText", BaseConstants.Cut);
			CopyText = GetSystemText("CopyText", BaseConstants.Copy);
			PasteText = GetSystemText("PasteText", BaseConstants.Paste);
			ShowCodeText = GetSystemText("ShowCode", BaseConstants.ShowCodeDefaultValue);
			ShowNameText = GetSystemText("ShowName", BaseConstants.ShowNameDefaultValue);
            ValidatingSpecifiedTextText = GetSystemText("ValidatingSpecifiedText");
        }

		#endregion Public

		#region Protected

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		protected virtual async Task LoadBusinessObject() { }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        /// <summary>
        /// Validates the code when the textbox loses focus
        /// </summary>
        protected virtual void LostFocusValidateCatalogCode() { }

        /// <summary>
        /// Command executed when the Open With Context Menu is clicked.
        /// </summary>
        protected virtual async Task OpenViewAsync()
        {
            string message;

			try
			{
				if (string.IsNullOrEmpty(AssemblyName))
				{
					message = await GetText("EmptyAssemblyName", BaseConstants.SpecifySmallerTextBoxTextDefaultValue,
						ClassName);

					BaseViewModel.ShowInformationMessage(message);
					return;
				}

				if (string.IsNullOrEmpty(ViewName))
				{
					message = await GetText("EmptyViewName", BaseConstants.EmptyViewNameDefaultValue,
						ClassName);

					BaseViewModel.ShowInformationMessage(message);
					return;
				}

				message = await GetText("LoadingBusinessObject", BaseConstants.LoadingBusinessObjectDefaultValue, ClassName);

				BaseViewModel.SetViewIsBusy(message);

				dynamic view = ViewUtil.IsWindowOpen(ViewName);

				if (view != null)
				{
					view.DataContext.LoadModel(SelectedBusinessObject);

					if (view.WindowState == WindowState.Minimized)
					{
						view.WindowState = WindowState.Normal;
					}

					view.Focus();
					return;
				}

				view = await BaseViewModel.CreateInstance(ViewName, AssemblyName);
				//view.Width = BaseViewModel.ConstWidthView;
				//view.Height = BaseViewModel.ConstHeightView;

				view.Show();

				//The LoadModel Method is implemented in the BaseCRUDViewModel class.
				view.DataContext.LoadModel(SelectedBusinessObject);
			}
			catch(Exception)
			{
				throw;
			}
			finally
			{
				BaseViewModel.ViewIsBusy = false;
			}
        }

		protected virtual async Task TextChangedAsync()
		{
			if (SelectedText != null && SelectedText.Length > TextSize + 1)
			{
				//If the Text exceeded the Length, we clear the Text, because the OldValue is very likely
				//to be unexistent as well. For instance, the oldValue should be ppppppppp
				//as this event is fired every time the text changes.

				var message = await GetText("FieldExceededSize", BaseConstants.FieldExceededSizeDefaultValue,
                    ClassName);
				message = string.Format(message, TextSize);

				ShowFlyOutMessage(message);
				return;
			}

			if (!AcceptsText && !string.IsNullOrEmpty(SelectedText))
			{
				SelectedText = string.Empty;
			}

			//If the Code typed by the User is Empty, set the Selected Model as Empty.
			if (SelectedText == null || string.IsNullOrEmpty(SelectedText))
			{
				SelectedBusinessObject = null;
			}
		}

		/// <summary>
		/// Gets the Value of the Property specified in the DisplayMemberPath Property.
		/// </summary>
		/// <returns>Value of the Property specified in the DisplayMemberPath Property.</returns>
		protected virtual string GetPropertyValue(ISimpleBusinessObject businessObject = null)
		{
			var baseBusinessObject = businessObject == null ? SelectedBusinessObject : businessObject;

			if(baseBusinessObject == null)
			{
				return string.Empty;
			}

			var property = BusinessObjectProperties.FirstOrDefault(x => x.Name == DisplayMemberPath);

            //If the Business Object is a Document, and DisplayMemberPath = Name, and the Name Property is excluded from the Mapping of
            //the Document, then we change DisplayMemberPath to Folio.
            if (ReflectionUtil.TypeHaveBaseType(baseBusinessObject.GetType(), typeof(Document)))
            {
                if (DisplayMemberPath == BaseConstants.Name && property != null && ReflectionUtil.HaveAttribute(property, typeof(ExcludeMappingAttribute)))
                {
                    DisplayMemberPath = BaseConstants.Folio;
                    property = BusinessObjectProperties.FirstOrDefault(x => x.Name == DisplayMemberPath);
                }
            }

            object value = null;

			if(property != null)
			{
				value = property.GetValue(baseBusinessObject);

				return value == null ? string.Empty : value.ToString();
			}

			property = BusinessObjectProperties.FirstOrDefault(x => x.Name == BaseConstants.Code);

			if (property != null)
			{
				value = property.GetValue(baseBusinessObject);

				return value == null ? string.Empty : value.ToString();
			}

			property = BusinessObjectProperties.FirstOrDefault(x => x.Name == BaseConstants.Name);

			if (property != null)
			{
				value = property.GetValue(baseBusinessObject);

				return value == null ? string.Empty : value.ToString();
			}

			return string.Empty;
		}

		#endregion Protected

		#region Private

		private async void TextChanged()
		{
			await TextChangedAsync();
		}

		private async void OpenView()
		{
			await OpenViewAsync();
		}

		private void ChangeDisplayMemberPath()
		{
			if (DisplayMemberPath == BaseConstants.Code)
			{
				DisplayMemberPath = BaseConstants.Name;
			}
			else if (DisplayMemberPath == BaseConstants.Name)
			{
				DisplayMemberPath = BaseConstants.Code;
			}

			var propertyValue = GetPropertyValue(SelectedBusinessObject);
			SelectedText = propertyValue;

			NotifyOfPropertyChange(() => ShowCodeOrNameIcon);
			NotifyOfPropertyChange(() => ShowCodeOrNameText);
		}

		#endregion Private

		#endregion Methods

		#region Events

		/// <summary>
		/// Callback executed when the Selected model changes.
		/// </summary>
		/// <param name="sender">Object sender of the event</param>
		/// <param name="e">Event parameters</param>
		private static void SelectedModelChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as BusinessObjectFinder;

            if (control == null)
            {
                return;
            }

            if (e.NewValue == null)
            {
                if (!string.IsNullOrEmpty(control.SelectedText))
                {
                    control.SelectedText = string.Empty;
                }

                if (!string.IsNullOrEmpty(control.SelectedId))
                {
                    control.SelectedId = string.Empty;
                }

                return;
            }

            var selectedModel = (SimpleBusinessObject)e.NewValue;

            if (selectedModel == null)
            {
                if (!string.IsNullOrEmpty(control.SelectedText))
                {
                    control.SelectedText = string.Empty;
                }

                if (!string.IsNullOrEmpty(control.SelectedId))
                {
                    control.SelectedId = string.Empty;
                }

                return;
            }

			var propertyValue = control.GetPropertyValue(selectedModel);

            if (string.IsNullOrEmpty(propertyValue) && selectedModel.Id > 0)
            {
                //await control.LoadBusinessObject();
                //control.SelectedCode = control.SelectedBusinessObject.Code;
            }
            else
            {
                control.SelectedText = propertyValue;
            }

            control.SelectedId = selectedModel.Id.ToString();
            control.NotifyOfPropertyChange(() => control.SelectedText);
            control.NotifyOfPropertyChange(() => control.SelectedId);
            control.NotifyOfPropertyChange(() => control.SelectedBusinessObjectName);
        }

        /// <summary>
        /// Callback executed when the SelectedText Property changes.
        /// </summary>
        /// <param name="sender">Object sender of the event</param>
        /// <param name="e">Event parameters</param>
        private static void SelectedTextChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as BusinessObjectFinder;

            if (control == null)
            {
                return;
            }

            control.TextChanged();
        }

        /// <summary>
        /// Callback executed when the ReadOnlyMode Property changes.
        /// </summary>
        /// <param name="sender">Object sender of the event</param>
        /// <param name="e">Event parameters</param>
        private static void ReadOnlyModeChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as BusinessObjectFinder;

            if (control == null)
            {
                return;
            }

            //On Read Only Mode, hide the ClearTextButton.
            control.ClearTextButton = !(bool)e.NewValue;
		}

        /// <summary>
        /// Callback executed when the TextBoxReadOnly Property changes.
        /// </summary>
        /// <param name="sender">Object sender of the event</param>
        /// <param name="e">Event parameters</param>
        private static void TextBoxReadOnlyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as BusinessObjectFinder;

            if (control == null)
            {
                return;
            }

            control.ReadOnlyMode = (bool)e.NewValue;

			if (control.TextBoxReadOnly)
			{
				control.ButtonImg.IsEnabled = !control.ReadOnlyMode;
			}
		}

		protected override void Instance_EditPermissionUpdatedEvent(object sender, EventArgs e)
        {
            base.Instance_EditPermissionUpdatedEvent(sender, e);

            if (!ReadOnlyMode)
            {
                ReadOnlyMode = !SessionVariables.Instance.UserHaveEditPermission;
            }
        }

        private void ButtonImg_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => ButtonActualWidth);
        }

        #endregion Events
    }
}
