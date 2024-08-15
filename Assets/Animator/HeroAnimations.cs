using Animancer;
using GameClient;
using GameClient.Battle;
using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroAnimations : MonoBehaviour
{

    public enum HState
    {
        None,
        Idle,
        Run,
        Attack,
        Die,
        Gethit,
        SkillIntonate,  //技能蓄力中
        SkillActive,    //技能已激活
    }

    private Skill skill;

    public HState state = HState.Idle;

    GameEntity gameEntity;
    NamedAnimancerComponent _animancer;

    void Start()
    {
        gameEntity = GetComponent<GameEntity>();
        _animancer = GetComponent<NamedAnimancerComponent>();

        Kaiyun.Event.RegisterOut("EntitySync", this, "EntitySync");
        Kaiyun.Event.RegisterOut("OnSkillIntonate", this, "OnSkillIntonate");
        Kaiyun.Event.RegisterOut("OnSkillActive", this, "OnSkillActive");
    }

    public void EntitySync(NetEntitySync entitySync)
    {
        int id = entitySync.Entity.Id;
        if (id != gameEntity.entityId) return;

        switch (entitySync.State)
        {
            case EntityState.Idle: PlayIdle(); break;
            case EntityState.Move: PlayRun(); break;
        }
    }

    public void OnSkillIntonate(Skill skill)
    {
        if (gameEntity.entityId != skill.actor.entityId) return;
        this.skill = skill;
        this.state = HState.SkillIntonate;
        Play(skill.Def.Anim1);
    }

    public void OnSkillActive(Skill skill)
    {
        if (gameEntity.entityId != skill.actor.entityId) return;
        this.skill = skill;
        this.state = HState.SkillActive;
        Play(skill.Def.Anim2);
    }


    public AnimancerState Play(string name, Action OnEnd = null)
    {
        if (_animancer == null)
            return null;
        AnimancerState state = _animancer.TryPlay(name);
        if (state == null)
        {
            Debug.LogWarning($"动画名称不存在[{name}]");
        }
        else
        {
            if (OnEnd != null)
            {
                state.Events.OnEnd = OnEnd;
            }
        }
        return state;
    }

    // Update is called once per frame
    void Update()
    {
        if (skill != null)
        {
            //施法动作
            if (skill.skillStage == Skill.Stage.Intonate)
            {
                Play(skill.Def.Anim1);
            }
            else if (skill.skillStage == Skill.Stage.Active)
            {
                Play(skill.Def.Anim2);
            }
            //恢复动作
            if (state == HState.SkillIntonate && skill.skillStage != Skill.Stage.Intonate)
            {
                state = HState.Idle;
                PlayIdle();
            }
            if (state == HState.SkillActive && skill.skillStage != Skill.Stage.Active)
            {
                state = HState.Idle;
                PlayIdle();
            }
        }

        if (gameEntity.actor.IsDeath)
        {
            state = HState.Die;
            Play("Die");
        }

        if (state == HState.Die && !gameEntity.actor.IsDeath)
        {
            state = HState.Idle;
            Play("Idle");
        }

    }


    public void PlayIdle()
    {
        if (state == HState.Attack
            || state == HState.Gethit
            || state == HState.Die
            || state == HState.SkillIntonate
            || state == HState.SkillActive)
            return;
        Play("Idle");
        state = HState.Idle;
        gameEntity.entityState = Proto.EntityState.Idle;
    }

    public void PlayRun()
    {
        if (state == HState.Attack
            || state == HState.SkillIntonate
            || state == HState.SkillActive
            )
            return;
        Play("Run");
        state = HState.Run;
        gameEntity.entityState = Proto.EntityState.Move;
    }

    public void PlayAttack()
    {
        Play("Attack", OnEnd: OnAttackEnd);
        state = HState.Attack;
    }


    public void PlayDie()
    {
        Play("Die");
        state = HState.Die;
    }

    public void PlayGethit()
    {
        Play("GetHit", GethitEnd);
        state = HState.Gethit;
    }


    public void OnAttackEnd()
    {
        state = HState.None;
        PlayIdle();
    }

    public void GethitEnd()
    {
        state = HState.None;
        PlayIdle();
    }

}
