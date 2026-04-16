using Godot;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Superstitio.Main.Features.HangingCard.UI;

// ReSharper disable NullableWarningSuppressionIsUsed
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

public partial class NHangingCardQueue : Control
{
    public static NHangingCardQueue? Instance
    {
        get => IsInstanceValid(field) ? field : null;
        protected set;
    }


    public List<HangingCardDisplay> ActiveCards { get; init; } = [];

    // 间距属性，允许外部动态调整
    public float CardSpacing
    {
        get;
        set
        {
            field = value;
            this.UpdateLayout();
        }
    } = 85f;

    public static NHangingCardQueue EnsureCreated()
    {
        if (Instance is not null)
            return Instance;

        Instance = new NHangingCardQueue
        {
            Name = nameof(NHangingCardQueue),
            // 默认放在屏幕底部中央或你喜欢的位置
            AnchorLeft = 0.5f,
            AnchorRight = 0.5f,
            AnchorTop = 0.3f,
            AnchorBottom = 0.3f,
        };

        NCombatRoom.Instance?.AddChild(Instance);

        return Instance;
    }

    /// <summary>
    /// 添加卡牌到队列并初始化其位置
    /// </summary>
    public HangingCardDisplay AddCard(HangingCardToken token)
    {
        var display = HangingCardDisplay.Create(token);
        this.ActiveCards.Add(display);
        this.AddChild(display);

        // 立即执行一次布局，确保新卡牌知道它该去哪
        this.UpdateLayout();

        // 初始化时直接瞬移到队列位置，防止从原点(0,0)飞过来
        display.GlobalPosition = display.QueuePosition;

        return display;
    }

    /// <summary>
    /// 从逻辑队列移除，并触发 Display 内部的删除状态机
    /// </summary>
    public void RemoveCard(HangingCardToken token)
    {
        var display = this.ActiveCards.FirstOrDefault(d => d.Token == token);
        if (display is null) return;

        this.ActiveCards.Remove(display);

        // 调用 Display 自己的状态机切换（它会自己处理淡出和销毁）
        display.Command_Remove();

        // 剩余卡牌平滑重排
        this.UpdateLayout();
    }

    /// <summary>
    /// 核心布局逻辑：仅更新每个 Display 的 QueuePosition
    /// 至于 Display 如何移动到这个位置，由它自己的状态机决定
    /// </summary>
    public void UpdateLayout()
    {
        if (this.ActiveCards.Count == 0) return;

        // 获取 Queue 节点在屏幕上的真实中心点
        var baseGlobalPos = this.GlobalPosition;

        // 计算总宽度，居中排列
        float totalWidth = (this.ActiveCards.Count - 1) * this.CardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < this.ActiveCards.Count; i++)
        {
            // 更新锚点位置。注意：我们只给它赋值目标，不直接操作 Position
            // 这样就解耦了布局计算和动画表现
            this.ActiveCards[i].QueuePosition = baseGlobalPos + new Vector2(startX + i * this.CardSpacing, 0);
        }
    }

    // 如果战斗房间大小改变，可能需要重新计算位置
    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            this.UpdateLayout();
        }
    }
}