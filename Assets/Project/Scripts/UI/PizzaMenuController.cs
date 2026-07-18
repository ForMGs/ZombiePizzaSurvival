using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public sealed class PizzaMenuController : MonoBehaviour
{
    private UIDocument document;
    private Button ordersButton;
    private Button mapButton;
    private VisualElement menuRoot;
    private VisualElement mapView;
    private VisualElement mapCanvas;
    private VisualElement ordersPage;
    private VisualElement mapPage;
    private bool isMenuVisible = true;
    private float mapZoom = 1f;

    private const float MinMapZoom = 1f;
    private const float MaxMapZoom = 2.5f;
    private const float MapZoomStep = 0.15f;

    private void OnEnable()
    {
        document = GetComponent<UIDocument>();
        VisualElement root = document.rootVisualElement;

        menuRoot = root.Q<VisualElement>("MainController");
        mapView = root.Q<VisualElement>("LocalMapView");
        mapCanvas = root.Q<VisualElement>("MapCanvas");
        ordersButton = root.Q<Button>("DailyMenuButton");
        mapButton = root.Q<Button>("MapButton");
        ordersPage = root.Q<VisualElement>("OrdersPage");
        mapPage = root.Q<VisualElement>("MapPage");

        if (ordersButton != null) ordersButton.clicked += ShowOrders;
        if (mapButton != null) mapButton.clicked += ShowMap;
        mapView?.RegisterCallback<WheelEvent>(OnMapWheel);

        ShowMap();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    private void OnDisable()
    {
        if (ordersButton != null) ordersButton.clicked -= ShowOrders;
        if (mapButton != null) mapButton.clicked -= ShowMap;
        mapView?.UnregisterCallback<WheelEvent>(OnMapWheel);
    }

    private void ShowOrders() => SelectPage(ordersPage, ordersButton);
    private void ShowMap() => SelectPage(mapPage, mapButton);

    private void ToggleMenu()
    {
        if (menuRoot == null) return;

        isMenuVisible = !isMenuVisible;
        menuRoot.style.display = isMenuVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnMapWheel(WheelEvent evt)
    {
        if (mapCanvas == null) return;

        float direction = evt.delta.y < 0f ? 1f : -1f;
        mapZoom = Mathf.Clamp(mapZoom + direction * MapZoomStep, MinMapZoom, MaxMapZoom);
        mapCanvas.style.scale = new Scale(new Vector3(mapZoom, mapZoom, 1f));
        evt.StopPropagation();
    }

    private void SelectPage(VisualElement selectedPage, Button selectedButton)
    {
        if (ordersPage == null || mapPage == null) return;

        ordersPage.style.display = selectedPage == ordersPage ? DisplayStyle.Flex : DisplayStyle.None;
        mapPage.style.display = selectedPage == mapPage ? DisplayStyle.Flex : DisplayStyle.None;

        ordersButton?.RemoveFromClassList("tab-button--selected");
        mapButton?.RemoveFromClassList("tab-button--selected");
        selectedButton?.AddToClassList("tab-button--selected");
    }
}
