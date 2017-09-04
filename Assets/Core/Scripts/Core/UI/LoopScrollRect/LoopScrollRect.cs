﻿using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public abstract class LoopScrollRect : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler,
        IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup
    {
        //==========LoopScrollRect==========

        public enum MovementType
        {
            Unrestricted, // Unrestricted movement -- can scroll forever
            Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
            Clamped // Restricted movement where it's not possible to go past the edges
        }

        public enum ScrollbarVisibility
        {
            Permanent,
            AutoHide,
            AutoHideAndExpandViewport
        }

        private readonly Vector3[] m_Corners = new Vector3[4];
        public int preCount = 1;
        //==========LoopScrollRect==========

        //public LoopScrollPrefabSource prefabSource;
        public RectTransform prefab = null;

        [Tooltip("Reverse direction for dragging")]
        public bool reverseDirection = false;

        [Tooltip("Rubber scale for outside")]
        public float rubberScale = 1;

        [Tooltip("Threshold for preloading")]
        public float threshold = 100;

        [Tooltip("Total count, negative means INFINITE mode")]
        public int totalCount;

        protected int directionSign = 0;
        protected int itemTypeEnd;

        protected int itemTypeStart;

        protected GridLayoutGroup m_GridLayout;

        [SerializeField]
        private RectTransform m_Content;

        private Bounds m_ContentBounds;

        private int m_ContentConstraintCount;

        private float m_ContentSpacing = -1;
        private Vector2 m_ContentStartPosition = Vector2.zero;

        [SerializeField]
        private ScrollDataProvider m_DataProvider = new ScrollDataProvider();

        [SerializeField]
        private float m_DecelerationRate = 0.135f; // Only used when inertia is enabled

        private bool m_Dragging;

        [SerializeField]
        private float m_Elasticity = 0.1f; // Only used for MovementType.Elastic

        [NonSerialized]
        private bool m_HasRebuiltLayout;

        [SerializeField]
        private bool m_Horizontal = true;

        [SerializeField]
        private Scrollbar m_HorizontalScrollbar;

        private RectTransform m_HorizontalScrollbarRect;

        [SerializeField]
        private float m_HorizontalScrollbarSpacing;

        [SerializeField]
        private ScrollbarVisibility m_HorizontalScrollbarVisibility;

        private bool m_HSliderExpand;
        private float m_HSliderHeight;

        [SerializeField]
        private bool m_Inertia = true;

        [SerializeField]
        private MovementType m_MovementType = MovementType.Elastic;

        [SerializeField]
        private ScrollRectEvent m_OnValueChanged = new ScrollRectEvent();

        // The offset from handle position to mouse down position
        private Vector2 m_PointerStartLocalCursor = Vector2.zero;
        private Bounds m_PrevContentBounds;

        private Vector2 m_PrevPosition = Vector2.zero;
        private Bounds m_PrevViewBounds;

        [NonSerialized]
        private RectTransform m_Rect;

        [SerializeField]
        private float m_ScrollSensitivity = 1.0f;

        private DrivenRectTransformTracker m_Tracker;

        private Vector2 m_Velocity;

        [SerializeField]
        private bool m_Vertical = true;

        [SerializeField]
        private Scrollbar m_VerticalScrollbar;

        private RectTransform m_VerticalScrollbarRect;

        [SerializeField]
        private float m_VerticalScrollbarSpacing;

        [SerializeField]
        private ScrollbarVisibility m_VerticalScrollbarVisibility;

        private Bounds m_ViewBounds;

        [SerializeField]
        private RectTransform m_Viewport;

        private RectTransform m_ViewRect;
        private bool m_VSliderExpand;
        private float m_VSliderWidth;
        private LoopScrollItemPool pool;

        protected LoopScrollRect()
        {
            flexibleWidth = -1;
        }

        public LoopScrollItemPool prefabPool
        {
            get
            {
                if(null == pool)
                {
                    pool = new LoopScrollItemPool(content, prefab, preCount);
                }
                return pool;
            }
        }

        protected float contentSpacing
        {
            get
            {
                if(m_ContentSpacing >= 0)
                {
                    return m_ContentSpacing;
                }
                m_ContentSpacing = 0;
                if(content != null)
                {
                    var layout1 = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
                    if(layout1 != null)
                    {
                        m_ContentSpacing = layout1.spacing;
                    }
                    m_GridLayout = content.GetComponent<GridLayoutGroup>();
                    if(m_GridLayout != null)
                    {
                        m_ContentSpacing = GetDimension(m_GridLayout.spacing);
                    }
                }
                return m_ContentSpacing;
            }
        }

        protected int contentConstraintCount
        {
            get
            {
                if(m_ContentConstraintCount > 0)
                {
                    return m_ContentConstraintCount;
                }
                m_ContentConstraintCount = 1;
                if(content != null)
                {
                    var layout2 = content.GetComponent<GridLayoutGroup>();
                    if(layout2 != null)
                    {
                        if(layout2.constraint == GridLayoutGroup.Constraint.Flexible)
                        {
                            Debug.LogWarning("[LoopScrollRect] Flexible not supported yet");
                        }
                        m_ContentConstraintCount = layout2.constraintCount;
                    }
                }
                return m_ContentConstraintCount;
            }
        }

        public RectTransform content { get { return m_Content; } set { m_Content = value; } }

        public bool horizontal { get { return m_Horizontal; } set { m_Horizontal = value; } }

        public bool vertical { get { return m_Vertical; } set { m_Vertical = value; } }

        public MovementType movementType { get { return m_MovementType; } set { m_MovementType = value; } }

        public float elasticity { get { return m_Elasticity; } set { m_Elasticity = value; } }

        public bool inertia { get { return m_Inertia; } set { m_Inertia = value; } }

        public float decelerationRate { get { return m_DecelerationRate; } set { m_DecelerationRate = value; } }

        public float scrollSensitivity { get { return m_ScrollSensitivity; } set { m_ScrollSensitivity = value; } }

        public RectTransform viewport
        {
            get { return m_Viewport; }
            set
            {
                m_Viewport = value;
                SetDirtyCaching();
            }
        }

        public Scrollbar horizontalScrollbar
        {
            get { return m_HorizontalScrollbar; }
            set
            {
                if(m_HorizontalScrollbar)
                {
                    m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
                }
                m_HorizontalScrollbar = value;
                if(m_HorizontalScrollbar)
                {
                    m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
                }
                SetDirtyCaching();
            }
        }

        public Scrollbar verticalScrollbar
        {
            get { return m_VerticalScrollbar; }
            set
            {
                if(m_VerticalScrollbar)
                {
                    m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
                }
                m_VerticalScrollbar = value;
                if(m_VerticalScrollbar)
                {
                    m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
                }
                SetDirtyCaching();
            }
        }

        public ScrollbarVisibility horizontalScrollbarVisibility
        {
            get { return m_HorizontalScrollbarVisibility; }
            set
            {
                m_HorizontalScrollbarVisibility = value;
                SetDirtyCaching();
            }
        }

        public ScrollbarVisibility verticalScrollbarVisibility
        {
            get { return m_VerticalScrollbarVisibility; }
            set
            {
                m_VerticalScrollbarVisibility = value;
                SetDirtyCaching();
            }
        }

        public float horizontalScrollbarSpacing
        {
            get { return m_HorizontalScrollbarSpacing; }
            set
            {
                m_HorizontalScrollbarSpacing = value;
                SetDirty();
            }
        }

        public float verticalScrollbarSpacing
        {
            get { return m_VerticalScrollbarSpacing; }
            set
            {
                m_VerticalScrollbarSpacing = value;
                SetDirty();
            }
        }

        public ScrollRectEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        public ScrollDataProvider DataProvidger { get { return m_DataProvider; } set { m_DataProvider = value; } }

        protected RectTransform viewRect
        {
            get
            {
                if(m_ViewRect == null)
                {
                    m_ViewRect = m_Viewport;
                }
                if(m_ViewRect == null)
                {
                    m_ViewRect = (RectTransform) transform;
                }
                return m_ViewRect;
            }
        }

        public Vector2 velocity { get { return m_Velocity; } set { m_Velocity = value; } }

        private RectTransform rectTransform
        {
            get
            {
                if(m_Rect == null)
                {
                    m_Rect = GetComponent<RectTransform>();
                }
                return m_Rect;
            }
        }

        public Vector2 normalizedPosition
        {
            get { return new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition); }
            set
            {
                SetNormalizedPosition(value.x, 0);
                SetNormalizedPosition(value.y, 1);
            }
        }

        public float horizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                //==========LoopScrollRect==========
                if(totalCount > 0 && itemTypeEnd > itemTypeStart)
                {
                    //TODO: consider contentSpacing
                    var elementSize = m_ContentBounds.size.x/(itemTypeEnd - itemTypeStart);
                    var totalSize = elementSize*totalCount;
                    var offset = m_ContentBounds.min.x - elementSize*itemTypeStart;
                    if(totalSize <= m_ViewBounds.size.x)
                    {
                        return m_ViewBounds.min.x > offset ? 1 : 0;
                    }
                    return (m_ViewBounds.min.x - offset)/(totalSize - m_ViewBounds.size.x);
                }
                return 0.5f;
                //==========LoopScrollRect==========
            }
            set { SetNormalizedPosition(value, 0); }
        }

        public float verticalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                //==========LoopScrollRect==========
                if(totalCount > 0 && itemTypeEnd > itemTypeStart)
                {
                    //TODO: consider contentSpacinge
                    var elementSize = m_ContentBounds.size.y/(itemTypeEnd - itemTypeStart);
                    var totalSize = elementSize*totalCount;
                    var offset = m_ContentBounds.max.y + elementSize*itemTypeStart;
                    if(totalSize <= m_ViewBounds.size.y)
                    {
                        return offset > m_ViewBounds.max.y ? 1 : 0;
                    }
                    return (offset - m_ViewBounds.max.y)/(totalSize - m_ViewBounds.size.y);
                }
                return 0.5f;
                //==========LoopScrollRect==========
            }
            set { SetNormalizedPosition(value, 1); }
        }

        private bool hScrollingNeeded
        {
            get
            {
                if(Application.isPlaying)
                {
                    return m_ContentBounds.size.x > m_ViewBounds.size.x + 0.01f;
                }
                return true;
            }
        }

        private bool vScrollingNeeded
        {
            get
            {
                if(Application.isPlaying)
                {
                    return m_ContentBounds.size.y > m_ViewBounds.size.y + 0.01f;
                }
                return true;
            }
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if(eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            if(!IsActive())
            {
                return;
            }
            UpdateBounds();
            m_PointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position,
                eventData.pressEventCamera, out m_PointerStartLocalCursor);
            m_ContentStartPosition = m_Content.anchoredPosition;
            m_Dragging = true;
        }

        //==========LoopScrollRect==========

        public virtual void Rebuild(CanvasUpdate executing)
        {
            if(executing == CanvasUpdate.Prelayout)
            {
                UpdateCachedData();
            }
            if(executing == CanvasUpdate.PostLayout)
            {
                UpdateBounds(false);
                UpdateScrollbars(Vector2.zero);
                UpdatePrevData();
                m_HasRebuiltLayout = true;
            }
        }

        public virtual void LayoutComplete()
        {
        }

        public virtual void GraphicUpdateComplete()
        {
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if(eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            if(!IsActive())
            {
                return;
            }
            Vector2 localCursor;
            if(
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position,
                    eventData.pressEventCamera, out localCursor))
            {
                return;
            }
            UpdateBounds();
            var pointerDelta = localCursor - m_PointerStartLocalCursor;
            var position = m_ContentStartPosition + pointerDelta;

            // Offset to get content into place in the view.
            var offset = CalculateOffset(position - m_Content.anchoredPosition);
            position += offset;
            if(m_MovementType == MovementType.Elastic)
            {
                //==========LoopScrollRect==========
                if(offset.x != 0)
                {
                    position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x)*rubberScale;
                }
                if(offset.y != 0)
                {
                    position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y)*rubberScale;
                }
                //==========LoopScrollRect==========
            }
            SetContentAnchoredPosition(position);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if(eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            m_Dragging = false;
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if(eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            m_Velocity = Vector2.zero;
        }

        public virtual void CalculateLayoutInputHorizontal()
        {
        }

        public virtual void CalculateLayoutInputVertical()
        {
        }

        public virtual float minWidth { get { return -1; } }

        public virtual float preferredWidth { get { return -1; } }

        public virtual float flexibleWidth { get; private set; }

        public virtual float minHeight { get { return -1; } }

        public virtual float preferredHeight { get { return -1; } }

        public virtual float flexibleHeight { get { return -1; } }

        public virtual int layoutPriority { get { return -1; } }

        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();
            if(m_HSliderExpand || m_VSliderExpand)
            {
                m_Tracker.Add(this, viewRect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.SizeDelta |
                    DrivenTransformProperties.AnchoredPosition);

                // Make view full size to see if content fits.
                viewRect.anchorMin = Vector2.zero;
                viewRect.anchorMax = Vector2.one;
                viewRect.sizeDelta = Vector2.zero;
                viewRect.anchoredPosition = Vector2.zero;

                // Recalculate content layout with this size to see if it fits when there are no scrollbars.
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if(m_VSliderExpand && vScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);

                // Recalculate content layout with this size to see if it fits vertically
                // when there is a vertical scrollbar (which may reflowed the content to make it taller).
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit horizontally, enable horizontal scrollbar and shrink view vertically to make room for it.
            if(m_HSliderExpand && hScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(viewRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If the vertical slider didn't kick in the first time, and the horizontal one did,
            // we need to check again if the vertical slider now needs to kick in.
            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if(m_VSliderExpand && vScrollingNeeded && viewRect.sizeDelta.x == 0 && viewRect.sizeDelta.y < 0)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);
            }
        }

        public virtual void SetLayoutVertical()
        {
            UpdateScrollbarLayout();
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();
        }

        public virtual void OnScroll(PointerEventData data)
        {
            if(!IsActive())
            {
                return;
            }
            EnsureLayoutHasRebuilt();
            UpdateBounds();
            var delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if(vertical && !horizontal)
            {
                if(Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    delta.y = delta.x;
                }
                delta.x = 0;
            }
            if(horizontal && !vertical)
            {
                if(Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                {
                    delta.x = delta.y;
                }
                delta.y = 0;
            }
            var position = m_Content.anchoredPosition;
            position += delta*m_ScrollSensitivity;
            if(m_MovementType == MovementType.Clamped)
            {
                position += CalculateOffset(position - m_Content.anchoredPosition);
            }
            SetContentAnchoredPosition(position);
            UpdateBounds();
        }

        protected abstract float GetSize(RectTransform item);
        protected abstract float GetDimension(Vector2 vector);
        protected abstract Vector2 GetVector(float value);

        protected virtual bool UpdateItems(Bounds viewBounds, Bounds contentBounds)
        {
            return false;
        }

        //==========LoopScrollRect==========
        private void ReturnObjectAndSendMessage(Transform go)
        {
            //go.SendMessage("ScrollCellReturn", SendMessageOptions.DontRequireReceiver);
            //prefabSource.ReturnObjectToPool(go.gameObject);
            prefabPool.Release(content, (RectTransform) go);
        }

        public void ClearCells()
        {
            if(Application.isPlaying)
            {
                itemTypeStart = 0;
                itemTypeEnd = 0;
                totalCount = 0;
                for(var i = content.childCount - 1; i >= 0; i--)
                {
                    ReturnObjectAndSendMessage(content.GetChild(i));
                }
            }
        }

        public void RefreshCells()
        {
            if(Application.isPlaying && isActiveAndEnabled)
            {
                itemTypeEnd = itemTypeStart;
                // recycle items if we can
                for(var i = 0; i < content.childCount; i++)
                {
                    if(itemTypeEnd < totalCount)
                    {
                        m_DataProvider.Invoke(content.GetChild(i), itemTypeEnd);
                        itemTypeEnd++;
                    }
                    else
                    {
                        ReturnObjectAndSendMessage(content.GetChild(i));
                        i--;
                    }
                }
            }
        }

        public void RefillCellsFromEnd(int offset = 0)
        {
            //TODO: unsupported for Infinity or Grid yet
            if(!Application.isPlaying || totalCount < 0 || contentConstraintCount > 1 || prefab == null)
            {
                return;
            }

            //prefabSource.InitPool();
            //prefabPool.Init(content, prefab, preCount);
            StopMovement();
            itemTypeEnd = reverseDirection ? offset : totalCount - offset;
            itemTypeStart = itemTypeEnd;
            for(var i = m_Content.childCount - 1; i >= 0; i--)
            {
                ReturnObjectAndSendMessage(m_Content.GetChild(i));
            }
            float sizeToFill = 0, sizeFilled = 0;
            if(directionSign == -1)
            {
                sizeToFill = viewRect.rect.size.y;
            }
            else
            {
                sizeToFill = viewRect.rect.size.x;
            }
            while(sizeToFill > sizeFilled)
            {
                var size = reverseDirection ? NewItemAtEnd() : NewItemAtStart();
                if(size <= 0)
                {
                    break;
                }
                sizeFilled += size;
            }
            var pos = m_Content.anchoredPosition;
            var dist = Mathf.Max(0, sizeFilled - sizeToFill);
            if(reverseDirection)
            {
                dist = -dist;
            }
            if(directionSign == -1)
            {
                pos.y = dist;
            }
            else if(directionSign == 1)
            {
                pos.x = dist;
            }
            m_Content.anchoredPosition = pos;
        }

        public void RefillCells(int offset = 0)
        {
            if(!Application.isPlaying || prefab == null)
            {
                return;
            }

            //prefabSource.InitPool();
            //prefabPool.Init(content, prefab, preCount);
            StopMovement();
            itemTypeStart = reverseDirection ? totalCount - offset : offset;
            itemTypeEnd = itemTypeStart;

            // Don't `Canvas.ForceUpdateCanvases();` here, or it will new/delete cells to change itemTypeStart/End
            for(var i = m_Content.childCount - 1; i >= 0; i--)
            {
                ReturnObjectAndSendMessage(m_Content.GetChild(i));
            }
            float sizeToFill = 0, sizeFilled = 0;
            // m_ViewBounds may be not ready when RefillCells on Start
            if(directionSign == -1)
            {
                sizeToFill = viewRect.rect.size.y;
            }
            else
            {
                sizeToFill = viewRect.rect.size.x;
            }
            while(sizeToFill > sizeFilled)
            {
                var size = reverseDirection ? NewItemAtStart() : NewItemAtEnd();
                if(size <= 0)
                {
                    break;
                }
                sizeFilled += size;
            }
            var pos = m_Content.anchoredPosition;
            if(directionSign == -1)
            {
                pos.y = 0;
            }
            else if(directionSign == 1)
            {
                pos.x = 0;
            }
            m_Content.anchoredPosition = pos;
        }

        protected float NewItemAtStart()
        {
            if(totalCount >= 0 && itemTypeStart - contentConstraintCount < 0)
            {
                return 0;
            }
            float size = 0;
            for(var i = 0; i < contentConstraintCount; i++)
            {
                itemTypeStart--;
                var newItem = InstantiateNextItem(itemTypeStart);
                newItem.SetAsFirstSibling();
                size = Mathf.Max(GetSize(newItem), size);
            }
            if(!reverseDirection)
            {
                var offset = GetVector(size);
                content.anchoredPosition += offset;
                m_PrevPosition += offset;
                m_ContentStartPosition += offset;
            }
            return size;
        }

        protected float DeleteItemAtStart()
        {
            if((totalCount >= 0 && itemTypeEnd >= totalCount - 1) || content.childCount == 0)
            {
                return 0;
            }
            float size = 0;
            for(var i = 0; i < contentConstraintCount; i++)
            {
                var oldItem = content.GetChild(0) as RectTransform;
                size = Mathf.Max(GetSize(oldItem), size);
                ReturnObjectAndSendMessage(oldItem);
                itemTypeStart++;
                if(content.childCount == 0)
                {
                    break;
                }
            }
            if(!reverseDirection)
            {
                var offset = GetVector(size);
                content.anchoredPosition -= offset;
                m_PrevPosition -= offset;
                m_ContentStartPosition -= offset;
            }
            return size;
        }

        protected float NewItemAtEnd()
        {
            if(totalCount >= 0 && itemTypeEnd >= totalCount)
            {
                return 0;
            }
            float size = 0;
            // issue 4: fill lines to end first
            var count = contentConstraintCount - content.childCount%contentConstraintCount;
            for(var i = 0; i < count; i++)
            {
                var newItem = InstantiateNextItem(itemTypeEnd);
                size = Mathf.Max(GetSize(newItem), size);
                itemTypeEnd++;
                if(totalCount >= 0 && itemTypeEnd >= totalCount)
                {
                    break;
                }
            }
            if(reverseDirection)
            {
                var offset = GetVector(size);
                content.anchoredPosition -= offset;
                m_PrevPosition -= offset;
                m_ContentStartPosition -= offset;
            }
            return size;
        }

        protected float DeleteItemAtEnd()
        {
            if((totalCount >= 0 && itemTypeStart < contentConstraintCount) || content.childCount == 0)
            {
                return 0;
            }
            float size = 0;
            for(var i = 0; i < contentConstraintCount; i++)
            {
                var oldItem = content.GetChild(content.childCount - 1) as RectTransform;
                size = Mathf.Max(GetSize(oldItem), size);
                ReturnObjectAndSendMessage(oldItem);
                itemTypeEnd--;
                if(itemTypeEnd%contentConstraintCount == 0 || content.childCount == 0)
                {
                    break; //just delete the whole row
                }
            }
            if(reverseDirection)
            {
                var offset = GetVector(size);
                content.anchoredPosition += offset;
                m_PrevPosition += offset;
                m_ContentStartPosition += offset;
            }
            return size;
        }

        private RectTransform InstantiateNextItem(int itemIdx)
        {
            var nextItem = (RectTransform) prefabPool.Get(content, prefab);
            //prefabSource.GetObject().GetComponent<RectTransform>();
            //nextItem.transform.SetParent(content, false);
            nextItem.gameObject.SetActive(true);
            m_DataProvider.Invoke(nextItem, itemIdx);
            return nextItem;
        }

        private void UpdateCachedData()
        {
            var transform = this.transform;
            m_HorizontalScrollbarRect = m_HorizontalScrollbar == null
                ? null
                : m_HorizontalScrollbar.transform as RectTransform;
            m_VerticalScrollbarRect = m_VerticalScrollbar == null
                ? null
                : m_VerticalScrollbar.transform as RectTransform;

            // These are true if either the elements are children, or they don't exist at all.
            var viewIsChild = viewRect.parent == transform;
            var hScrollbarIsChild = !m_HorizontalScrollbarRect || m_HorizontalScrollbarRect.parent == transform;
            var vScrollbarIsChild = !m_VerticalScrollbarRect || m_VerticalScrollbarRect.parent == transform;
            var allAreChildren = viewIsChild && hScrollbarIsChild && vScrollbarIsChild;
            m_HSliderExpand = allAreChildren && m_HorizontalScrollbarRect &&
                              horizontalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_VSliderExpand = allAreChildren && m_VerticalScrollbarRect &&
                              verticalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_HSliderHeight = m_HorizontalScrollbarRect == null ? 0 : m_HorizontalScrollbarRect.rect.height;
            m_VSliderWidth = m_VerticalScrollbarRect == null ? 0 : m_VerticalScrollbarRect.rect.width;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if(m_HorizontalScrollbar)
            {
                m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
            }
            if(m_VerticalScrollbar)
            {
                m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
            }
            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
            if(m_HorizontalScrollbar)
            {
                m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
            }
            if(m_VerticalScrollbar)
            {
                m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
            }
            m_HasRebuiltLayout = false;
            m_Tracker.Clear();
            m_Velocity = Vector2.zero;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        public override bool IsActive()
        {
            return base.IsActive() && m_Content != null;
        }

        private void EnsureLayoutHasRebuilt()
        {
            if(!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
            {
                Canvas.ForceUpdateCanvases();
            }
        }

        public virtual void StopMovement()
        {
            m_Velocity = Vector2.zero;
        }

        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            if(!m_Horizontal)
            {
                position.x = m_Content.anchoredPosition.x;
            }
            if(!m_Vertical)
            {
                position.y = m_Content.anchoredPosition.y;
            }
            if(position != m_Content.anchoredPosition)
            {
                m_Content.anchoredPosition = position;
                UpdateBounds();
            }
        }

        protected virtual void LateUpdate()
        {
            if(!m_Content)
            {
                return;
            }
            EnsureLayoutHasRebuilt();
            UpdateScrollbarVisibility();
            UpdateBounds();
            var deltaTime = Time.unscaledDeltaTime;
            var offset = CalculateOffset(Vector2.zero);
            if(!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero))
            {
                var position = m_Content.anchoredPosition;
                for(var axis = 0; axis < 2; axis++)
                {
                    // Apply spring physics if movement is elastic and content has an offset from the view.
                    if(m_MovementType == MovementType.Elastic && offset[axis] != 0)
                    {
                        var speed = m_Velocity[axis];
                        position[axis] = Mathf.SmoothDamp(m_Content.anchoredPosition[axis],
                            m_Content.anchoredPosition[axis] + offset[axis], ref speed, m_Elasticity, Mathf.Infinity,
                            deltaTime);
                        m_Velocity[axis] = speed;
                    }
                    // Else move content according to velocity with deceleration applied.
                    else if(m_Inertia)
                    {
                        m_Velocity[axis] *= Mathf.Pow(m_DecelerationRate, deltaTime);
                        if(Mathf.Abs(m_Velocity[axis]) < 1)
                        {
                            m_Velocity[axis] = 0;
                        }
                        position[axis] += m_Velocity[axis]*deltaTime;
                    }
                    // If we have neither elaticity or friction, there shouldn't be any velocity.
                    else
                    {
                        m_Velocity[axis] = 0;
                    }
                }
                if(m_Velocity != Vector2.zero)
                {
                    if(m_MovementType == MovementType.Clamped)
                    {
                        offset = CalculateOffset(position - m_Content.anchoredPosition);
                        position += offset;
                    }
                    SetContentAnchoredPosition(position);
                }
            }
            if(m_Dragging && m_Inertia)
            {
                Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition)/deltaTime;
                m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime*10);
            }
            if(m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds ||
               m_Content.anchoredPosition != m_PrevPosition)
            {
                UpdateScrollbars(offset);
                m_OnValueChanged.Invoke(normalizedPosition);
                UpdatePrevData();
            }
        }

        private void UpdatePrevData()
        {
            if(m_Content == null)
            {
                m_PrevPosition = Vector2.zero;
            }
            else
            {
                m_PrevPosition = m_Content.anchoredPosition;
            }
            m_PrevViewBounds = m_ViewBounds;
            m_PrevContentBounds = m_ContentBounds;
        }

        private void UpdateScrollbars(Vector2 offset)
        {
            if(m_HorizontalScrollbar && horizontal)
            {
                //==========LoopScrollRect==========
                if(m_ContentBounds.size.x > 0 && totalCount > 0)
                {
                    m_HorizontalScrollbar.size =
                        Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x))/m_ContentBounds.size.x*
                                      (itemTypeEnd - itemTypeStart)/totalCount);
                }
                //==========LoopScrollRect==========
                else
                {
                    m_HorizontalScrollbar.size = 1;
                }
                m_HorizontalScrollbar.value = horizontalNormalizedPosition;
            }
            if(m_VerticalScrollbar && vertical)
            {
                //==========LoopScrollRect==========
                if(m_ContentBounds.size.y > 0 && totalCount > 0)
                {
                    m_VerticalScrollbar.size =
                        Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y))/m_ContentBounds.size.y*
                                      (itemTypeEnd - itemTypeStart)/totalCount);
                }
                //==========LoopScrollRect==========
                else
                {
                    m_VerticalScrollbar.size = 1;
                }
                m_VerticalScrollbar.value = verticalNormalizedPosition;
            }
        }

        private void SetHorizontalNormalizedPosition(float value)
        {
            SetNormalizedPosition(value, 0);
        }

        private void SetVerticalNormalizedPosition(float value)
        {
            SetNormalizedPosition(value, 1);
        }

        private void SetNormalizedPosition(float value, int axis)
        {
            //==========LoopScrollRect==========
            if(totalCount <= 0 || itemTypeEnd <= itemTypeStart)
            {
                return;
            }
            //==========LoopScrollRect==========
            EnsureLayoutHasRebuilt();
            UpdateBounds();

            //==========LoopScrollRect==========
            var localPosition = m_Content.localPosition;
            var newLocalPosition = localPosition[axis];
            if(axis == 0)
            {
                var elementSize = m_ContentBounds.size.x/(itemTypeEnd - itemTypeStart);
                var totalSize = elementSize*totalCount;
                var offset = m_ContentBounds.min.x - elementSize*itemTypeStart;
                newLocalPosition += m_ViewBounds.min.x - value*(totalSize - m_ViewBounds.size[axis]) - offset;
            }
            else if(axis == 1)
            {
                var elementSize = m_ContentBounds.size.y/(itemTypeEnd - itemTypeStart);
                var totalSize = elementSize*totalCount;
                var offset = m_ContentBounds.max.y + elementSize*itemTypeStart;
                newLocalPosition -= offset - value*(totalSize - m_ViewBounds.size.y) - m_ViewBounds.max.y;
            }
            //==========LoopScrollRect==========
            if(Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
            {
                localPosition[axis] = newLocalPosition;
                m_Content.localPosition = localPosition;
                m_Velocity[axis] = 0;
                UpdateBounds();
            }
        }

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - 1/(Mathf.Abs(overStretching)*0.55f/viewSize + 1))*viewSize*Mathf.Sign(overStretching);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private void UpdateScrollbarVisibility()
        {
            if(m_VerticalScrollbar && m_VerticalScrollbarVisibility != ScrollbarVisibility.Permanent &&
               m_VerticalScrollbar.gameObject.activeSelf != vScrollingNeeded)
            {
                m_VerticalScrollbar.gameObject.SetActive(vScrollingNeeded);
            }
            if(m_HorizontalScrollbar && m_HorizontalScrollbarVisibility != ScrollbarVisibility.Permanent &&
               m_HorizontalScrollbar.gameObject.activeSelf != hScrollingNeeded)
            {
                m_HorizontalScrollbar.gameObject.SetActive(hScrollingNeeded);
            }
        }

        private void UpdateScrollbarLayout()
        {
            if(m_VSliderExpand && m_HorizontalScrollbar && m_Horizontal)
            {
                m_Tracker.Add(this, m_HorizontalScrollbarRect,
                    DrivenTransformProperties.AnchorMinX |
                    DrivenTransformProperties.AnchorMaxX |
                    DrivenTransformProperties.SizeDeltaX |
                    DrivenTransformProperties.AnchoredPositionX);
                m_HorizontalScrollbarRect.anchorMin = new Vector2(0, m_HorizontalScrollbarRect.anchorMin.y);
                m_HorizontalScrollbarRect.anchorMax = new Vector2(1, m_HorizontalScrollbarRect.anchorMax.y);
                m_HorizontalScrollbarRect.anchoredPosition = new Vector2(0, m_HorizontalScrollbarRect.anchoredPosition.y);
                if(vScrollingNeeded)
                {
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing),
                        m_HorizontalScrollbarRect.sizeDelta.y);
                }
                else
                {
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(0, m_HorizontalScrollbarRect.sizeDelta.y);
                }
            }
            if(m_HSliderExpand && m_VerticalScrollbar && m_Vertical)
            {
                m_Tracker.Add(this, m_VerticalScrollbarRect,
                    DrivenTransformProperties.AnchorMinY |
                    DrivenTransformProperties.AnchorMaxY |
                    DrivenTransformProperties.SizeDeltaY |
                    DrivenTransformProperties.AnchoredPositionY);
                m_VerticalScrollbarRect.anchorMin = new Vector2(m_VerticalScrollbarRect.anchorMin.x, 0);
                m_VerticalScrollbarRect.anchorMax = new Vector2(m_VerticalScrollbarRect.anchorMax.x, 1);
                m_VerticalScrollbarRect.anchoredPosition = new Vector2(m_VerticalScrollbarRect.anchoredPosition.x, 0);
                if(hScrollingNeeded)
                {
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x,
                        -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                }
                else
                {
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0);
                }
            }
        }

        private void UpdateBounds(bool updateItems = true)
        {
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();
            if(m_Content == null)
            {
                return;
            }

            // ============LoopScrollRect============
            // Don't do this in Rebuild
            if(Application.isPlaying && updateItems && UpdateItems(m_ViewBounds, m_ContentBounds))
            {
                Canvas.ForceUpdateCanvases();
                m_ContentBounds = GetBounds();
            }
            // ============LoopScrollRect============

            // Make sure content bounds are at least as large as view by adding padding if not.
            // One might think at first that if the content is smaller than the view, scrolling should be allowed.
            // However, that's not how scroll views normally work.
            // Scrolling is *only* possible when content is *larger* than view.
            // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
            // E.g. if pivot is at top, bounds are expanded downwards.
            // This also works nicely when ContentSizeFitter is used on the content.
            var contentSize = m_ContentBounds.size;
            var contentPos = m_ContentBounds.center;
            var excess = m_ViewBounds.size - contentSize;
            if(excess.x > 0)
            {
                contentPos.x -= excess.x*(m_Content.pivot.x - 0.5f);
                contentSize.x = m_ViewBounds.size.x;
            }
            if(excess.y > 0)
            {
                contentPos.y -= excess.y*(m_Content.pivot.y - 0.5f);
                contentSize.y = m_ViewBounds.size.y;
            }
            m_ContentBounds.size = contentSize;
            m_ContentBounds.center = contentPos;
        }

        private Bounds GetBounds()
        {
            if(m_Content == null)
            {
                return new Bounds();
            }
            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            var toLocal = viewRect.worldToLocalMatrix;
            m_Content.GetWorldCorners(m_Corners);
            for(var j = 0; j < 4; j++)
            {
                var v = toLocal.MultiplyPoint3x4(m_Corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }
            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        private Vector2 CalculateOffset(Vector2 delta)
        {
            var offset = Vector2.zero;
            if(m_MovementType == MovementType.Unrestricted)
            {
                return offset;
            }
            Vector2 min = m_ContentBounds.min;
            Vector2 max = m_ContentBounds.max;
            if(m_Horizontal)
            {
                min.x += delta.x;
                max.x += delta.x;
                if(min.x > m_ViewBounds.min.x)
                {
                    offset.x = m_ViewBounds.min.x - min.x;
                }
                else if(max.x < m_ViewBounds.max.x)
                {
                    offset.x = m_ViewBounds.max.x - max.x;
                }
            }
            if(m_Vertical)
            {
                min.y += delta.y;
                max.y += delta.y;
                if(max.y < m_ViewBounds.max.y)
                {
                    offset.y = m_ViewBounds.max.y - max.y;
                }
                else if(min.y > m_ViewBounds.min.y)
                {
                    offset.y = m_ViewBounds.min.y - min.y;
                }
            }
            return offset;
        }

        protected void SetDirty()
        {
            if(!IsActive())
            {
                return;
            }
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected void SetDirtyCaching()
        {
            if(!IsActive())
            {
                return;
            }
            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirtyCaching();
        }
#endif

        [Serializable]
        public class ScrollRectEvent : UnityEvent<Vector2>
        {
        }

        [Serializable]
        public class ScrollDataProvider : UnityEvent<Transform, int>
        {
        }
    }
}