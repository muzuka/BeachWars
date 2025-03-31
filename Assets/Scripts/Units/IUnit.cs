using UnityEngine;

public interface IUnit
{
    public void SetController(Player player);
    public void SetAttacker(GameObject enemy);
    public void UpdateUI(InfoViewController gui);
    public void Deselect();
    public void ToggleSelected();
    public void Destroyed();
    public void EnemyDied();
}
