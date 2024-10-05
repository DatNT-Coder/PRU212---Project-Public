using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour {
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;
    public NetworkVariable<int> CurrentPlayerhealth = new NetworkVariable<int>();
    private bool isDead;

    public Action<Health> OnDie;

    public override void OnNetworkSpawn() {
        if(!IsServer) return;
        CurrentPlayerhealth.Value = MaxHealth;
    }

    public void TakeDamage(int damageValue) {
        ModifyHealth(-damageValue);
    }

    public void RestoreHealth(int healValue) {
        ModifyHealth(healValue);
    }

    private void ModifyHealth(int healthValue) {
        if(isDead) return;

        int newHealth = CurrentPlayerhealth.Value + healthValue;
        CurrentPlayerhealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth);

        if (CurrentPlayerhealth.Value == 0) {
            OnDie?.Invoke(this);
            isDead = true;
        }
    }
}
