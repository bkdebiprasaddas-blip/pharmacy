# UI demonstration video

The generated deliverable is `deliverables/FreshBasket_UI_Feature_Demo.mp4`:

- 2 minutes 18 seconds; 1920×1080, 24 fps, H.264/yuv420p.
- Silent, with burned-in English captions, chapter titles, focus highlights, pointer animation, and transitions.
- Covers 19 scenes: storefront, registration/login, catalog/search, product details, cart, COD checkout, order confirmation/history, admin dashboard, categories, products, stock editing, customers, order status, and reports.
- A matching `.srt` caption file is included in `deliverables`.

**This is a source-based UI walkthrough with illustrative data, not a recording of the running ASP.NET/Access application.** That distinction is burned into every scene and included in the MP4 metadata. Backend operations are described from source, not claimed to be verified. No live payment or database action is performed.

The renderer expands the actual ASPX control templates using explicit sample data and reuses `Content/site.css` and `Content/grocery.svg`. Shared navigation is reproduced from the master pages. It uses the application's local CSS fallback rather than fetching Bootstrap from its CDN. The footage is not a pixel-perfect assertion about a particular Windows browser/runtime.

## Regenerate on Linux x64

Requires Node.js 22+, Python 3.9+, and access to npm/PyPI. These commands install isolated, ignored tooling; they do not change the application's runtime stack.

```sh
npm install --prefix .video-tools --no-audit --no-fund playwright@1.63.0 @sparticuz/chromium@152.0.0
pip install --target .video-tools/python pillow==12.3.0 imageio-ffmpeg==0.6.0 beautifulsoup4

PYTHONPATH=.video-tools/python python3 scripts/video/build_previews.py
node scripts/video/render_frames.mjs
PYTHONPATH=.video-tools/python python3 scripts/video/encode_video.py
```

`render_frames.mjs` uses the bundled Chromium library archive to supply missing NSS dependencies in this sandbox. It loads only local generated preview documents; it does not serve or execute the Web Forms application.

Tooling, intermediate frames, and video output are excluded from Git through `.gitignore`. The small regeneration scripts are retained. Generated videos should be distributed as downloadable artifacts rather than application source.

## Verification performed

- All 19 preview scenes rendered successfully.
- Preview HTML checked for unresolved ASPX tags/data-binding expressions.
- Representative source screenshots and an extracted encoded frame visually reviewed.
- Entire MP4 decoded successfully: 3,312 frames, 138 seconds, 1080p, H.264, 24 fps.
- Existing 10 application source-contract tests still pass.

Windows compilation, ACE integration, and live browser acceptance testing remain outstanding as recorded in `docs/TESTING.md`.
