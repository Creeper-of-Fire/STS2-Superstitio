using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using Superstitio.Main.Maso;
using Superstitio.Main.Maso.Pools;

namespace Superstitio.Main.SubPool.UI;

/// <summary>
/// 
/// </summary>
// 这是一个标准的 Godot 容器，它会自动垂直排列子节点
public class MasoSubPoolSelector : VBoxContainer
{
    /// <summary>
    /// 
    /// </summary>
    public MasoSubPoolSelector BuildUI()
    {
        Log.Info("[MasoMod] Selector _Ready 开始执行...");
        
        // --- 强制视觉可见测试 ---
        var testLabel = new Label();
        testLabel.Text = "MASO UI 注入成功！";
        testLabel.Modulate = Colors.Red; // 红色醒目
        this.AddChild(testLabel);
        // -----------------------
        
        // 1. 设置 UI 基本样式
        // 强行设定最小尺寸，防止被原版 VBox 挤压到 0 高度
        this.CustomMinimumSize = new Vector2(300, 50); 
        this.SizeFlagsHorizontal = SizeFlags.ExpandFill; // 填满横向空间
        this.AddThemeConstantOverride("separation", 10); // 按钮之间的间距

        // 2. 动态生成勾选框
        // 遍历所有注册到 MasoCardPool 的子池
        var allPools = SubPoolManager.GetAllSubPools().ToList();
        
        
        foreach (var pool in allPools)
        {
            var checkBox = new CheckButton();
            checkBox.Text = pool.Id; // 可以后续给 SubCardPool 加个 DisplayName，但是本地化相关现在还没有弄
            
            checkBox.ButtonPressed = SubPoolManager.IsPoolEnabled<MasoCardPool>(pool);

            // 当玩家点击勾选框时触发
            // 2. 绑定点击事件，实时修改 Manager 里的状态
            checkBox.Toggled += (on) => {
                SubPoolManager.SetPoolEnabled<MasoCardPool>(pool, on);
                Log.Info($"[MasoMod] 状态更新: MasoCardPool 的 {checkBox.Text} = {on}");
            };

            this.AddChild(checkBox);
            
            Log.Info($"[MasoMod] 成功挂载子池开关: {checkBox.Text}");
        }

        // 初始隐藏，等选到 Maso 角色再显示
        this.Hide();

        return this;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    public void UpdateVisibility(CharacterModel? model)
    {
        // 使用 OrdinalIgnoreCase 进行大小写无关的检查
        bool isMaso = model is MasoCharacter;

        Log.Info($"[MasoMod] 切换角色: {model?.Id.Entry ?? string.Empty}, 是否显示 UI: {isMaso}");

        if (isMaso)
            this.Show();
        else
            this.Hide();
    }
}