using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkAnimationController)),
    RequireComponent(typeof(NetworkTransformReceive)),
    RequireComponent(typeof(NetworkCharacterControllerReceive))]

public class UserEntity : Entity {

    protected override void Initialize() {
        base.Initialize();
    }

    protected override void OnDeath() {
        base.OnDeath();
        _networkAnimationReceive.SetAnimationProperty((int)PlayerAnimationEvent.death, 1f, true);
    } 

    public override bool StartAutoAttacking() {
        if (base.StartAutoAttacking()) {
            _networkAnimationReceive.SetBool("atk01_" + _gear.WeaponAnim, true);
        }

        return true;
    }

    public override bool StopAutoAttacking() {
        if (base.StopAutoAttacking()) {
            _networkAnimationReceive.SetBool("atk01_" + _gear.WeaponAnim, false);
            if(!_networkCharacterControllerReceive.IsMoving()) {
                _networkAnimationReceive.SetBool("atkwait_" + _gear.WeaponAnim, true);
            }
        }

        return true;
    }

    protected override void OnHit(bool criticalHit) {
        base.OnHit(criticalHit);
    }

    public override float UpdateMAtkSpeed(int mAtkSpd) {
        float converted = base.UpdateMAtkSpeed(mAtkSpd);
        _networkAnimationReceive.SetMAtkSpd(converted);

        return converted;
    }

    public override float UpdatePAtkSpeed(int pAtkSpd) {
        float converted = base.UpdatePAtkSpeed(pAtkSpd);
        _networkAnimationReceive.SetPAtkSpd(converted);

        return converted;
    }

    public override float UpdateSpeed(int speed) {
        float converted = base.UpdateSpeed(speed);
        _networkAnimationReceive.SetMoveSpeed(converted);

        return converted;
    }
}