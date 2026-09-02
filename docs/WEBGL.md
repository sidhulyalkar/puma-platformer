# From Unity slice to the personal arcade

The Unity project is standalone. No changes to `sidhulyalkar.com` are part of this initial development pass.

## Build and run

Install Web Build Support for Unity 6000.3.22f1. Use **Wildbound → Build WebGL**, or set `UNITY_EDITOR` and run `bash tools/build-webgl.sh`. Output is `Builds/WebGL`; `WILDBOUND_BUILD_PATH` can override it. The build method throws on failure rather than printing a success message for a failed build.

```bash
python3 -m http.server 8000 --directory Builds/WebGL
```

Visit `http://localhost:8000`; don't open `index.html` with a `file://` URL. The custom template adds a deliberate load gesture, progress/error UI, canvas focus, in-canvas scroll suppression, fullscreen, and device-pixel-ratio cap. Gameplay is keyboard/gamepad only in this version.

## Hosting configuration

The initial build uses gzip plus decompression fallback. Use the filenames Unity emits. Do not rename `.unityweb` files or apply guessed `Content-Encoding` headers. Fallback simplifies initial static hosting but increases loader work; switch to native server decompression only after configuring the host correctly and measuring.

For native compressed builds, serve `.wasm` as `application/wasm` and supply the appropriate `Content-Encoding` for the actual compression format. Serve the loader HTML without long immutable caching so it cannot point at removed build files. Fingerprinted build assets can use long-lived caching. See [Unity's server guidance](https://docs.unity3d.com/6000.3/Documentation/Manual/webgl-deploying.html).

No multithreaded Web build or cross-origin-isolation dependency is intentionally introduced. Verify the generated build settings rather than assuming that an iframe host provides the required headers.

## Later arcade embed

After the browser checks pass, place the build at an agreed static URL and add an iframe to the site's arcade page, with a direct-play fallback link:

```html
<iframe
  src="/games/wildbound/index.html"
  title="Puma: Wildbound — exploration platformer"
  allow="fullscreen; gamepad; autoplay"
  allowfullscreen
  loading="lazy"
  style="width:100%;aspect-ratio:16/9;border:0"
></iframe>
```

This path is an example, not a deployed location. Test same-origin routing, focus handoff, fullscreen exit, sound consent, mobile messaging, and storage behavior in the actual site's layout. User saves are local and can be lost when browser data is cleared or the serving origin changes.

Prefer a playable, measured build and a short real-game recording over a launch page that promises unverified performance or features.
