/// <summary>
/// Obstáculo estático (vehículo abandonado). Permanece fijo en la plataforma donde apareció,
/// el jugador puede saltarlo y se destruye al recibir un proyectil.
/// Todo el comportamiento (matar al jugador y destruirse) se hereda de <see cref="ObjetoPeligroso"/>;
/// los distintos tipos de vehículo solo cambian de modelo y material en su prefab.
/// </summary>
public class VehiculoAbandonado : ObjetoPeligroso
{
}