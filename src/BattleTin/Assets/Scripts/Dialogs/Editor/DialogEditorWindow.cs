using System;
using System.Collections.Generic;
using System.Linq;
using MIDIFrogs.FutureInThePast.Editor.Dialogs;
using MIDIFrogs.FutureInThePast.Editor.Utilities;
using MIDIFrogs.FutureInThePast.Quests.Dialogs;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#pragma warning disable CS0618 // Editor SO warning

public class DialogEditorWindow : EditorWindow
{
    internal DialogGraphRepository repository;
    internal DialogAuthor selectedAuthor;
    internal Dialog selectedDialog;
    private const string AuthorsPath = "Assets/Resources/Dialogs/Authors";
    private const string DialogsPath = "Assets/Resources/Dialogs";
    private Tab authorsTab;
    private Tab dialogsTab;
    private DialogGraphView dialogView;
    private VisualElement root;
    private Tabs selectedTab;
    private ListView sideView;

    private enum Tabs
    { Dialogs, Authors }

    internal float SidebarWidth => rootVisualElement.Q<TwoPaneSplitView>()?.fixedPane?.resolvedStyle.width ?? 250;

    [MenuItem("Window/Dialog Editor")]
    public static void ShowWindow()
    {
        DialogEditorWindow window = GetWindow<DialogEditorWindow>("Dialog Editor");
        window.minSize = new Vector2(800, 600);
    }

    public void ClearSelectedDialog()
    {
        if (selectedTab == Tabs.Dialogs)
            sideView.ClearSelection();
        else
            OnDialogSelected(null);
    }

    private void AddAuthorOrDialog()
    {
        switch (selectedTab)
        {
            case Tabs.Dialogs:
                CreateNewDialog();
                break;

            case Tabs.Authors:
                CreateNewAuthor();
                break;
        }
    }

    private Tab AddAuthorsTab()
    {
        authorsTab = new Tab("Authors");
        authorsTab.selected += ReloadAuthors;
        return authorsTab;
    }

    private Tab AddDialogsTab()
    {
        dialogsTab = new Tab("Dialogs");
        dialogsTab.selected += ReloadDialogs;
        return dialogsTab;
    }

    private void AddStyles()
    {
        rootVisualElement.AddStyleSheets("Dialogs/DialogVariables.uss");
    }

    private void BindAuthorCard(VisualElement authorPanel, DialogAuthor author)
    {
        authorPanel.Clear();
        authorPanel.style.flexDirection = FlexDirection.Row;
        authorPanel.style.alignItems = Align.Center;
        Image avatarPreview = new()
        {
            sprite = author.Avatar
        };
        avatarPreview.style.width = avatarPreview.style.height = 24;
        authorPanel.Add(avatarPreview);
        ObjectField avatarField = new()
        {
            objectType = typeof(Sprite),
            value = author.Avatar,
        };
        avatarField.RegisterValueChangedCallback((e) =>
        {
            author.Avatar = avatarPreview.sprite = (Sprite)e.newValue;
        });
        avatarField.style.width = 20;
        authorPanel.Add(avatarField);
        RenamableCard nameCard = new(author.Name) { TextColor = author.SignColor };
        nameCard.OnRenamed += newText => author.Name = newText;
        nameCard.AddManipulator(new ContextualMenuManipulator(evt =>
        {
            evt.menu.AppendAction("Delete author", action =>
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(author));
                ReloadAuthors(authorsTab);
            });
        }));
        nameCard.style.alignSelf = Align.Center;
        nameCard.style.alignItems = Align.Center;
        authorPanel.Add(nameCard);
    }

    private void BindDialogCard(VisualElement dialogPanel, Dialog dialog)
    {
        dialogPanel.Clear();
        dialogPanel.style.flexDirection = FlexDirection.Row;
        dialogPanel.style.alignItems = Align.Center;
        RenamableCard nameCard = new(dialog.name);
        nameCard.OnRenamed += newText =>
        {
            string oldPath = AssetDatabase.GetAssetPath(dialog);
            string cachePath = DialogGraphRepository.GetCachePath(dialog);
            AssetDatabase.RenameAsset(oldPath, newText);
            if (AssetDatabase.AssetPathExists(cachePath))
                AssetDatabase.RenameAsset(cachePath, newText);
        };
        nameCard.AddManipulator(new ContextualMenuManipulator(evt =>
        {
            evt.menu.AppendAction("Delete dialog", action =>
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(dialog));
                string cachePath = DialogGraphRepository.GetCachePath(dialog);
                AssetDatabase.DeleteAsset(cachePath);
                ReloadDialogs(dialogsTab);
            });
        }));
        nameCard.style.alignSelf = Align.Stretch;
        nameCard.style.alignItems = Align.Center;
        dialogPanel.Add(nameCard);
    }

    private VisualElement CreateGraph()
    {
        dialogView = new(this);
        dialogView.SetEnabled(false);
        return dialogView;
    }

    private void CreateNewAuthor()
    {
        var newAuthor = CreateInstance<DialogAuthor>();
        newAuthor.Name = "<New Author>";
        newAuthor.SignColor = Color.white;
        newAuthor.Id = Guid.NewGuid().ToString();
        AssetDatabase.CreateAsset(newAuthor, $"{AuthorsPath}/NewAuthor.asset");
        AssetDatabase.SaveAssets();
        ReloadAuthors(authorsTab);
    }

    private void CreateNewDialog()
    {
        var newDialog = CreateInstance<Dialog>();
        AssetDatabase.CreateAsset(newDialog, $"{DialogsPath}/NewDialog.asset");
        AssetDatabase.SaveAssets();
        ReloadDialogs(dialogsTab);
    }

    private VisualElement CreateTabs()
    {
        var tabs = new TabView();
        var dialogs = AddDialogsTab();
        var authors = AddAuthorsTab();
        tabs.Add(dialogs);
        tabs.Add(authors);

        var header = tabs.Q("unity-tab-view__header-container");

        var addButton = new Button(AddAuthorOrDialog)
        {
            text = "+"
        };
        addButton.style.width = addButton.style.height = 20;
        addButton.style.marginLeft = Length.Auto();
        header.Add(addButton);

        return tabs;
    }

    private void CreateUI()
    {
        var splitView = new TwoPaneSplitView
        {
            orientation = TwoPaneSplitViewOrientation.Horizontal,
            fixedPaneInitialDimension = 250,
        };

        var tabsView = CreateTabs();
        ReloadDialogs(dialogsTab);
        splitView.Add(tabsView);
        var graphView = CreateGraph();
        splitView.Add(graphView);

        root.Add(splitView);
    }

    private void FocusInExplorer(UnityEngine.Object obj)
    {
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = obj; // Select the object in the project window
    }

    private void OnAuthorSelected(DialogAuthor author)
    {
        selectedAuthor = author;
        FocusInExplorer(author);
    }

    private void OnDialogSelected(Dialog dialog)
    {
        selectedDialog = dialog;
        if (dialog == null)
        {
            dialogView.ClearGraph();
            dialogView.SetEnabled(false);
            return;
        }

        repository = new DialogGraphRepository(dialogView, dialog);
        repository.Load();

        FocusInExplorer(dialog);
        dialogView.SetEnabled(true);
    }

    private void OnEnable()
    {
        // Create the root VisualElement
        root = rootVisualElement;

        // Create the UI elements
        CreateUI();
        AddStyles();
    }

    private void ReloadAuthors(Tab tab)
    {
        selectedTab = Tabs.Authors;
        tab.Clear();
        List<DialogAuthor> authors = ScanFor<DialogAuthor>(AuthorsPath, x => x.Name);
        var list = new List<DialogAuthor>();
        foreach (var author in authors)
        {
            list.Add(author);
        }
        sideView = new ListView(list, makeItem: () => new VisualElement(), bindItem: (x, i) => BindAuthorCard(x, list[i]));
        sideView.selectionChanged += (items) =>
        {
            OnAuthorSelected((DialogAuthor)items.First());
        };
        tab.Add(sideView);
    }

    private void ReloadDialogs(Tab tab)
    {
        selectedTab = Tabs.Dialogs;
        tab.Clear();
        List<Dialog> dialogs = ScanFor<Dialog>(DialogsPath);
        var list = new List<Dialog>();
        foreach (var dialog in dialogs)
        {
            list.Add(dialog);
        }
        sideView = new ListView(list, makeItem: () => new VisualElement(), bindItem: (x, i) => BindDialogCard(x, list[i]));
        sideView.selectionChanged += (items) =>
        {
            OnDialogSelected((Dialog)items.FirstOrDefault());
        };
        tab.Add(sideView);
    }

    private List<T> ScanFor<T>(string where, Func<T, string> compareKeyExtractor = null) where T : UnityEngine.Object
    {
        return AssetDatabase.FindAssets($"t:{typeof(T)}", new[] { where })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<T>)
            .OrderBy(x => compareKeyExtractor?.Invoke(x) ?? x.name)
            .ToList();
    }

    private static class ContextMenuBinder
    {
        private static readonly Dictionary<VisualElement, IManipulator> bindings = new();

        public static void Bind(VisualElement element, IManipulator binding)
        {
            bindings[element] = binding;
            element.AddManipulator(binding);
        }

        public static void Clear(VisualElement element)
        {
            if (bindings.Remove(element, out var binding))
                element.RemoveManipulator(binding);
        }
    }
}