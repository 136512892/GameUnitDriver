using UnityEngine;

public class SectorAttack
{
    /// <summary>
    /// 检测目标是否被扇形攻击命中
    /// </summary>
    /// <param name="sectorAngle">扇形的角度</param>
    /// <param name="sectorRadius">扇形的半径</param>
    /// <param name="attacker">攻击者</param>
    /// <param name="checkTarget">检测目标</param>
    /// <returns></returns>
    public static bool IsInRange(float sectorAngle, float sectorRadius, 
        Transform attacker, Transform checkTarget)
    {
        //攻击者指向检测目标的方向
        Vector3 direction = (checkTarget.position 
            - attacker.position).normalized;
        //点乘得到两个归一化向量之间的余弦值
        float dot = Vector3.Dot(attacker.forward, direction);
        //通过反余弦函数计算两个向量的夹角
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        return angle <= sectorAngle * .5f
            && direction.magnitude <= sectorRadius;
    }
}