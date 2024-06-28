using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오브젝트가 데미지를 받는 인터페이스
/// </summary>
public interface IDamage
{
    void TakeDamage(float amount);
}
