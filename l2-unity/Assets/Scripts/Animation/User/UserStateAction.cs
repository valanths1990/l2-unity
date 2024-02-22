using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserStateAction : UserStateBase
{
    public bool IsMoving() {
        return _networkCharacterControllerReceive.IsMoving();
    }

    public bool IsAttacking() {
        return _animator.GetBool("atk01" + _weaponType);
    }

    protected bool ShouldAtkWait() {
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (now - _entity.StopAutoAttackTime < 5000) {
            if (_entity.AttackTarget == null) {
                return true;
            }
        }

        return false;
    }
}
