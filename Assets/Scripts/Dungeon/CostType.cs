using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tipos de custo disponíveis para o pathfinding.
/// Cada tipo representa um recurso diferente que o jogador deve gerenciar.
/// </summary>
public enum CostType
{
    Health,  // Custo de vida (HP)
    Sanity,  // Custo de sanidade mental
    Time     // Custo de tempo (turnos/dias)
}
