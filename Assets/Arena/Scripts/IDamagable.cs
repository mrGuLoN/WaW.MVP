namespace Arena.Scripts
{
  public interface IDamagable
  {
    public float Health { get;protected set; }
    public void Damage(float damage);
   
  }
}
