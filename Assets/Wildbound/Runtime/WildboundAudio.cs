using UnityEngine;
using Wildbound.Core;

namespace Wildbound.Unity
{
    public sealed class WildboundAudio : MonoBehaviour
    {
        private WildboundGame game;
        private AudioSource source;
        private AudioClip jump, pounce, collect, bell, thump, claw, armor, hurt, moonwake;
        public void Initialize(WildboundGame owner)
        {
            game = owner; source = gameObject.AddComponent<AudioSource>(); source.playOnAwake = false;
            jump = Tone("leap", 360, 780, .14f); pounce = Tone("pounce", 160, 620, .23f);
            collect = Tone("mote", 880, 1320, .2f); bell = Tone("discovery", 523, 1046, .6f);
            thump = Tone("landing", 150, 65, .12f);
            claw = Whoosh(); armor = Tone("armor", 1250, 430, .12f);
            hurt = Tone("hurt", 230, 95, .2f); moonwake = Tone("moonwake", 659, 1318, .85f);
        }
        public void Wake() { if (!game.Muted) source.PlayOneShot(bell, .15f); }
        public void React(GameEvent events)
        {
            if (game.Muted) return;
            if ((events & (GameEvent.Bloom | GameEvent.Waystone | GameEvent.Balance)) != 0) source.PlayOneShot(moonwake, .26f);
            if ((events & GameEvent.Moonbell) != 0) source.PlayOneShot(bell, .25f);
            if ((events & GameEvent.Breach) != 0) source.PlayOneShot(thump, .3f);
            if ((events & GameEvent.Block) != 0) source.PlayOneShot(armor, .17f);
            else if ((events & GameEvent.Hit) != 0) source.PlayOneShot(thump, .24f);
            if ((events & GameEvent.Hurt) != 0) source.PlayOneShot(hurt, .24f);
            if ((events & (GameEvent.Claw | GameEvent.DashClaw)) != 0) source.PlayOneShot(claw, .22f);
            if ((events & GameEvent.Hunt) != 0) source.PlayOneShot(collect, .22f);
            if ((events & (GameEvent.Secret | GameEvent.Checkpoint | GameEvent.Portal)) != 0) source.PlayOneShot(bell, .3f);
            else if ((events & GameEvent.Collect) != 0) source.PlayOneShot(collect, .2f);
            if ((events & GameEvent.Pounce) != 0) source.PlayOneShot(pounce, .28f);
            else if ((events & (GameEvent.Jump | GameEvent.WallKick | GameEvent.Spring | GameEvent.Stomp)) != 0) source.PlayOneShot(jump, .2f);
            else if ((events & (GameEvent.Land | GameEvent.Respawn)) != 0) source.PlayOneShot(thump, .15f);
        }
        private static AudioClip Tone(string name, float start, float end, float seconds)
        {
            const int rate = 22050; int count = (int)(rate * seconds); var data = new float[count];
            float phase = 0;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count; phase += Mathf.Lerp(start, end, t) * 2 * Mathf.PI / rate;
                float envelope = Mathf.Min(1, t * 35) * Mathf.Pow(1 - t, 2);
                data[i] = (Mathf.Sin(phase) + .18f * Mathf.Sin(phase * 2)) * envelope * .5f;
            }
            AudioClip clip = AudioClip.Create(name, count, 1, rate, false); clip.SetData(data, 0); return clip;
        }
        private static AudioClip Whoosh()
        {
            const int rate = 22050, count = 4400; var samples = new float[count]; var random = new System.Random(42);
            float filtered = 0;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                filtered = Mathf.Lerp(filtered, (float)random.NextDouble() * 2 - 1, .22f);
                samples[i] = filtered * Mathf.Sin(t * Mathf.PI) * (1 - t);
            }
            var clip = AudioClip.Create("claw sweep", count, 1, rate, false); clip.SetData(samples, 0); return clip;
        }
        private void OnDestroy() { foreach (var clip in new[] { jump, pounce, collect, bell, thump, claw, armor, hurt, moonwake }) if (clip != null) Destroy(clip); }
    }
}
