#!/usr/bin/env python3
"""Render the authored region coordinates for design review, not a gameplay screenshot.

Requires matplotlib. Reads the literal construction calls in WorldRegions.cs; unsupported
coordinates fail loudly instead of silently producing an invented layout.
"""
import re
from pathlib import Path

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import Rectangle

ROOT = Path(__file__).resolve().parents[1]
SOURCE = (ROOT / "Assets/Wildbound/Core/WorldRegions.cs").read_text()
matplotlib.rcParams["svg.fonttype"] = "none"
PALETTE = {"ground": "#426167", "moon": "#8bd6e5", "trail": "#ffd58d", "spring": "#eb9ebd", "moving": "#bcacd9"}


def number(value):
    return float(value.strip().removesuffix("f"))


def points(text):
    return [(number(x), number(y)) for x, y in re.findall(r"new V2\(([-.\df]+), ([-.\df]+)\)", text)]


def draw():
    fig, axes = plt.subplots(3, 1, figsize=(14, 10.2), facecolor="#101b2a")
    titles = ["AMBER CANOPY  /  roots, branches, and a route back to shelter",
              "LANTERN GROTTO  /  switchback shelves around a high roost",
              "SKY GARDEN  /  springs, broad islands, and a moving perch"]
    for i, name in enumerate(("Canopy", "Grotto", "Sky")):
        ax = axes[i]
        body = SOURCE.split(f"private static void Build{name}(WorldDefinition w)", 1)[1]
        body = body.split("private static void", 1)[0]
        ax.set_facecolor("#101b2a")
        for args in re.findall(r"w\.Add\(([^;]+)\);", body):
            values = args.split(",")
            x, y, width = map(number, values[:3])
            height = number(values[3]) if len(values) > 3 else 1
            surface = values[4].strip() if len(values) > 4 else "Surface.Moss"
            color = PALETTE["spring"] if surface == "Surface.Spring" else PALETTE["moving"] if surface == "Surface.Moving" else PALETTE["ground"]
            ax.add_patch(Rectangle((x, y), width, height, color=color, alpha=.9))
            if len(values) > 5:
                travel = number(values[5]); ax.annotate("", (x + width / 2 + travel, y - .8), (x + width / 2 - travel, y - .8), arrowprops={"arrowstyle": "<->", "color": color})
        for method, key in (("Moonpath", "moon"), ("ReturnPath", "trail")):
            for args in re.findall(method + r"\(w, ([^;]+)\);", body):
                source, x, y, width = args.split(",")
                x, y, width = map(number, (x, y, width))
                ax.add_patch(Rectangle((x, y), width, .35, edgecolor=PALETTE[key], facecolor="none", linestyle="--", linewidth=1.5))
        for x, y, width, height in re.findall(r"w.Hazards.Add\(new Box\(([^,]+), ([^,]+), ([^,]+), ([^)]+)\)\);", body):
            ax.add_patch(Rectangle((number(x), number(y)), number(width), number(height), color="#e6858d"))
        pickup_call = re.search(r"PickupsAt\(w, (.*?)\);", body, re.S)
        pickup_points = points(pickup_call[1])
        assert len(pickup_points) == 13
        for index, (x, y) in enumerate(pickup_points):
            ax.plot(x, y, "D" if index == 0 else ".", color="#fff0c6", markersize=6 if index == 0 else 3)
        places = re.findall(r"w.Places.Add\(new WildPlace\((.*?)\)\);", body, re.S)
        assert len(places) == 2
        for j, place in enumerate(places):
            route = points(place)
            destination, clues = route[0], route[1:]
            ax.plot(*zip(*clues), ":", color="#b89961", linewidth=.8)
            ax.scatter(*zip(*clues), s=10, color="#c1a071")
            ax.annotate(str(i * 2 + j + 1), destination, xytext=(0, 14), textcoords="offset points", ha="center", fontsize=10, fontweight="bold", color="#101b2a", bbox={"boxstyle": "circle,pad=.25", "fc": PALETTE["trail"], "ec": "none"})
        for x in (23, 60):
            ax.plot(x, 1.4, "s", color="#e8f1e2", markersize=5)
        ax.plot(2, 1.5, "o", color="#e8ae67", markersize=6)
        ax.plot(75, 1.5, ">", color="#e8f1e2", markersize=7)
        ax.set(xlim=(-6, 82), ylim=(-3, 32 if i else 22))
        ax.set_aspect("equal", adjustable="box")
        ax.set_title(titles[i], loc="left", fontsize=12, color="#eff1e4", pad=14)
        ax.tick_params(colors="#73828f", labelsize=8)
        for spine in ax.spines.values():
            spine.set_visible(False)
        ax.grid(alpha=.07)
    fig.suptitle("WILDBOUND / LIVING TRAILS", color="#f4dfb1", fontsize=20, x=.08, ha="left", y=.98)
    fig.text(.08, .94, "Authored geometry schematic · all secrets shown · dashed paths begin dormant", color="#aebbc2", fontsize=11)
    fig.text(.08, .032, "1 Root Hollow   2 Amber Overlook   3 Stillwater Shelf   4 Lantern Roost   5 Cloud Nest   6 Starflower Crown", color="#ebd6ae", fontsize=10)
    fig.text(.08, .012, "Gold: discovery paths    Blue: moonbloom bridges    Pink: springs    Violet: moving perch    Diamond: memory    Square: shelter", color="#aebbc2", fontsize=9)
    fig.subplots_adjust(left=.08, right=.97, top=.89, bottom=.07, hspace=.37)
    fig.savefig(ROOT / "docs/living-trails-layouts.svg")
    fig.savefig(ROOT / "artifacts/living-trails-layouts.png", dpi=125)
    plt.close(fig)


if __name__ == "__main__":
    (ROOT / "artifacts").mkdir(exist_ok=True)
    draw()
