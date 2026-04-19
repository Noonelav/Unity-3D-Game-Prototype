using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;

public class CreatePlayerAnimator
{
    [MenuItem("Tools/Create Player Walk Animator")]
    public static void CreateAnimator()
    {
        string controllerPath = "Assets/_Project/Materials/PlayerWalkAnimator.controller";

        // 创建Animator Controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // 获取Root State Machine
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        // 创建Speed参数
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

        // 创建Idle状态
        AnimatorState idleState = rootStateMachine.AddState("Idle");
        idleState.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/_ThirdParty/BattleRoyaleDuoPAPBR/Animation/AssaultRifleStance/IdleNormal01_AR_Anim.fbx");

        // 创建Walk状态
        AnimatorState walkState = rootStateMachine.AddState("Walk");
        walkState.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(
            "Assets/_ThirdParty/BattleRoyaleDuoPAPBR/Animation/AssaultRifleStance/MoveFwd_AR_Anim.fbx");

        // 设置Idle为默认状态
        rootStateMachine.defaultState = idleState;

        // 创建过渡：Idle → Walk
        AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToWalk.duration = 0.2f;
        idleToWalk.exitTime = 0f;

        // 创建过渡：Walk → Idle
        AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        walkToIdle.duration = 0.2f;
        walkToIdle.exitTime = 0f;

        AssetDatabase.SaveAssets();
        Debug.Log("✓ PlayerWalkAnimator created successfully at: " + controllerPath);
    }
}
