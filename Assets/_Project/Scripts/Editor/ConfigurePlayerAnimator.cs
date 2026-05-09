using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;

public class ConfigurePlayerAnimator
{
    [MenuItem("Tools/Configure Player Walk Animator")]
    public static void ConfigureAnimator()
    {
        string controllerPath = "Assets/_Project/Materials/PlayerWalkController.controller";
        
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            Debug.LogError("PlayerWalkController not found!");
            return;
        }

        if (!HasParameter(controller, "Speed"))
        {
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        }

        if (!HasParameter(controller, "IsJumping"))
        {
            controller.AddParameter("IsJumping", AnimatorControllerParameterType.Bool);
        }

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

        // 删除所有旧状态（保留Entry和Exit）
        var statesToRemove = new System.Collections.Generic.List<AnimatorState>();
        foreach (var childState in stateMachine.states)
        {
            if (childState.state.name != "Entry" && childState.state.name != "Exit")
            {
                statesToRemove.Add(childState.state);
            }
        }
        
        foreach (var state in statesToRemove)
        {
            stateMachine.RemoveState(state);
        }

        // 创建Idle状态
        AnimatorState idleState = stateMachine.AddState("Idle");
        idleState.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/_ThirdParty/BattleRoyaleDuoPAPBR/Animation/AssaultRifleStance/IdleNormal01_AR_Anim.fbx");
        
        // 创建Walk状态
        AnimatorState walkState = stateMachine.AddState("Walk");
        walkState.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/_ThirdParty/BattleRoyaleDuoPAPBR/Animation/AssaultRifleStance/RunFWD_AR_Anim.fbx");

        // 创建Jump状态
        AnimatorState jumpState = stateMachine.AddState("Jump");
        AnimationClip jumpClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/_ThirdParty/BattleRoyaleDuoPAPBR/Animation/AssaultRifleStance/JumpInPlace_AR_Anim.fbx");
        if (jumpClip == null)
        {
            // 如果找不到Jump动画，使用Idle作为备选
            jumpClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                "Assets/_ThirdParty/BattleRoyaleDuoPAPBR/Animation/AssaultRifleStance/IdleNormal01_AR_Anim.fbx");
        }
        jumpState.motion = jumpClip;

        // 设置Idle为默认状态
        stateMachine.defaultState = idleState;

        // Idle → Walk过渡
        AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToWalk.duration = 0.2f;
        idleToWalk.exitTime = 0f;

        // Walk → Idle过渡
        AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        walkToIdle.duration = 0.2f;
        walkToIdle.exitTime = 0f;

        // Idle → Jump过渡
        AnimatorStateTransition idleToJump = idleState.AddTransition(jumpState);
        idleToJump.AddCondition(AnimatorConditionMode.If, 0f, "IsJumping");
        idleToJump.duration = 0.1f;
        idleToJump.exitTime = 0f;

        // Walk → Jump过渡
        AnimatorStateTransition walkToJump = walkState.AddTransition(jumpState);
        walkToJump.AddCondition(AnimatorConditionMode.If, 0f, "IsJumping");
        walkToJump.duration = 0.1f;
        walkToJump.exitTime = 0f;

        // Jump → Idle过渡 (IsJumping false 且 Speed < 0.1)
        AnimatorStateTransition jumpToIdle = jumpState.AddTransition(idleState);
        jumpToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsJumping");
        jumpToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        jumpToIdle.duration = 0.1f;
        jumpToIdle.exitTime = 0.7f;  // 等待跳跃动画大部分完成

        // Jump → Walk过渡 (IsJumping false 且 Speed > 0.1)
        AnimatorStateTransition jumpToWalk = jumpState.AddTransition(walkState);
        jumpToWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsJumping");
        jumpToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        jumpToWalk.duration = 0.1f;
        jumpToWalk.exitTime = 0.7f;

        // ========== 射击状态配置 ==========
        Debug.Log("🔫 Configuring Shoot state...");

        // 添加IsShooting参数（如果不存在）
        if (!HasParameter(controller, "IsShooting"))
        {
            controller.AddParameter("IsShooting", AnimatorControllerParameterType.Bool);
            Debug.Log("✓ IsShooting parameter added");
        }

        // 创建Shoot状态
        AnimatorState shootState = stateMachine.AddState("Shoot");
        Debug.Log("✓ Shoot state created");

        shootState.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/_ThirdParty/BattleRoyaleDuoPAPBR/Animation/AssaultRifleStance/ShootSingleshot_AR_Anim.fbx");
        Debug.Log("✓ Shoot animation assigned");

        // Idle → Shoot
        AnimatorStateTransition idleToShoot = idleState.AddTransition(shootState);
        idleToShoot.AddCondition(AnimatorConditionMode.If, 0f, "IsShooting");
        idleToShoot.duration = 0.05f;
        idleToShoot.exitTime = 0f;
        Debug.Log("✓ Idle → Shoot transition added");

        // Walk → Shoot
        AnimatorStateTransition walkToShoot = walkState.AddTransition(shootState);
        walkToShoot.AddCondition(AnimatorConditionMode.If, 0f, "IsShooting");
        walkToShoot.duration = 0.05f;
        walkToShoot.exitTime = 0f;
        Debug.Log("✓ Walk → Shoot transition added");

        // Shoot → Idle
        AnimatorStateTransition shootToIdle = shootState.AddTransition(idleState);
        shootToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsShooting");
        shootToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        shootToIdle.duration = 0.1f;
        shootToIdle.exitTime = 0.8f;
        Debug.Log("✓ Shoot → Idle transition added");

        // Shoot → Walk
        AnimatorStateTransition shootToWalk = shootState.AddTransition(walkState);
        shootToWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsShooting");
        shootToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        shootToWalk.duration = 0.1f;
        shootToWalk.exitTime = 0.8f;
        Debug.Log("✓ Shoot → Walk transition added");

        // 保存
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("✓ Controller saved");

        Debug.Log("✅ PlayerWalkController configured successfully!");
        Debug.Log("✓ Idle state created");
        Debug.Log("✓ Walk state created");
        Debug.Log("✓ Jump state created");
        Debug.Log("✓ IsJumping parameter added");
    }

    static bool HasParameter(AnimatorController controller, string parameterName)
    {
        foreach (var param in controller.parameters)
        {
            if (param.name == parameterName)
                return true;
        }
        return false;
    }
}
