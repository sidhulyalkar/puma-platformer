using System.Collections.Generic;
using UnityEngine;
using Wildbound.Core;

namespace Wildbound.Unity
{
    public sealed partial class WorldView
    {
        private sealed class TrackArt { public WildPlace Place; public V2 Point; public SpriteRenderer Scent; }
        private sealed class PlaceArt { public WildPlace Place; public SpriteRenderer Halo; public readonly List<SpriteRenderer> Flowers = new List<SpriteRenderer>(); }
        private readonly List<TrackArt> tracks = new List<TrackArt>();
        private readonly List<PlaceArt> places = new List<PlaceArt>();
        private readonly List<SpriteRenderer> memoryStars = new List<SpriteRenderer>();

        private void BuildExplorationArt()
        {
            tracks.Clear(); places.Clear(); memoryStars.Clear();
            var world = game.Session.World;
            if (game.Session.InTrial) return;
            foreach (var place in world.Places)
            {
                var art = new PlaceArt { Place = place };
                var root = new GameObject(place.Name).transform; root.SetParent(scenery); root.position = Point(place.Position);
                art.Halo = Shape("wild place glow", disc, new Vector2(0, .3f), new Vector2(2.5f, 1.3f), new Color(1, .8f, .4f, .08f), 4, root);
                // Low marks fit inside the root hollow and never suggest a solid obstacle.
                Shape("resting leaves", disc, new Vector2(0, .035f), new Vector2(1.35f, .1f), Hex("ab986f"), 9, root);
                for (int i = -2; i <= 2; i++)
                {
                    Shape("wild stem", square, new Vector2(i * .3f, .15f), new Vector2(.025f, .25f), moss, 10, root);
                    art.Flowers.Add(Shape("starflower", disc, new Vector2(i * .3f, .28f), Vector2.one * .1f, distant, 11, root));
                }
                places.Add(art);
                foreach (var point in place.Tracks)
                {
                    // Physical pawprints are always visible; scent adds local emphasis while stalking.
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Shape("pawprint pad", disc, new Vector2(point.X + side * .09f, point.Y + .045f), new Vector2(.1f, .055f), Hex("c8ad78"), 10);
                        Shape("pawprint toes", disc, new Vector2(point.X + side * .09f + .06f, point.Y + .09f), new Vector2(.06f, .04f), Hex("c8ad78"), 10);
                    }
                    var scent = Shape("trail scent", ring, new Vector2(point.X, point.Y + .25f), new Vector2(.65f, .4f), Hex("ffe0a0"), 14);
                    scent.enabled = false; tracks.Add(new TrackArt { Place = place, Point = point, Scent = scent });
                }
            }
            foreach (var checkpoint in world.Checkpoints)
            {
                // Shelter scenery stays behind the puma and agrees with the supporting ground.
                Shape("shelter shade", disc, new Vector2(checkpoint.X, checkpoint.Y + .6f), new Vector2(3.5f, 1.8f), new Color(.04f, .08f, .12f, .6f), 2);
                for (int i = 0; i < 3; i++)
                    memoryStars.Add(Shape("remembered star", disc, new Vector2(checkpoint.X + (i - 1) * .35f, checkpoint.Y + 2.15f + (i == 1 ? .2f : 0)), Vector2.one * .12f, distant, 13));
            }
        }
        private void UpdateExplorationArt()
        {
            var session = game.Session;
            foreach (var track in tracks)
            {
                track.Scent.enabled = !track.Place.Found && WildPlace.ScentVisible(session.World, session.Player, track.Point);
                float pulse = game.ReducedMotion ? 1 : 1 + .08f * Mathf.Sin(session.Time * 3);
                track.Scent.transform.localScale = new Vector3(.65f * pulse, .4f * pulse, 1);
            }
            foreach (var art in places)
            {
                art.Halo.color = new Color(1, .8f, .4f, art.Place.Found ? .19f : .04f);
                foreach (var flower in art.Flowers) flower.color = art.Place.Found ? Hex("ffe3a5") : distant;
            }
            for (int i = 0; i < memoryStars.Count; i++)
                memoryStars[i].color = (session.Save.Collected[i % 3] & 1) != 0 ? Hex("ffe3a5") : distant;
        }
    }
}
