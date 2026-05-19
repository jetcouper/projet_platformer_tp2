namespace TP2.Src;

public interface IHealthObserver
{
    void OnLivesChanged(int currentLives);
    void OnDamageTaken();
}
