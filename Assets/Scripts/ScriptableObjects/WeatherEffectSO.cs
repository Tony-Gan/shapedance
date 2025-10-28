using UnityEngine;

[CreateAssetMenu(fileName = "New Weather Effect", menuName = "Pokemon/Move Effect/Weather")]
public class WeatherEffectSO : MoveEffectSO
{
    [Header("Weather Details")]
    public WeatherType weatherType = WeatherType.None;
    public int duration = 5;
    public bool isPermanent = false;

    public override void Execute(PokemonStats caster, PokemonStats target)
    {
        if (Random.Range(0f, 1f) > probability)
        {
            return;
        }

        // TODO: 实现 BattleFieldManager 后，调用设置天气的方法
        // BattleFieldManager.Instance.SetWeather(weatherType, duration, isPermanent);
        
        Debug.Log($"[Effect] {GetWeatherMessage(weatherType)}");
    }

    private string GetWeatherMessage(WeatherType weather)
    {
        return weather switch
        {
            WeatherType.Sunny => "The sunlight turned harsh!",
            WeatherType.Rain => "It started to rain!",
            WeatherType.Sandstorm => "A sandstorm kicked up!",
            WeatherType.Hail => "It started to hail!",
            WeatherType.HarshSunlight => "The sunlight turned extremely harsh!",
            WeatherType.HeavyRain => "A heavy rain began to fall!",
            WeatherType.StrongWinds => "Mysterious strong winds blow!",
            WeatherType.None => "The weather cleared up!",
            _ => $"Weather changed to {weather}!"
        };
    }
}