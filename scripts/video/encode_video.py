"""Compose a silent, captioned 1080p MP4 from labeled source-based UI previews."""
from pathlib import Path
from PIL import Image, ImageDraw
import imageio_ffmpeg, json, subprocess, math
ROOT=Path(__file__).resolve().parents[2]
WORK=ROOT/'.video-work'
OUT=ROOT/'deliverables'
OUT.mkdir(exist_ok=True)
scenes=json.loads((WORK/'scenes.json').read_text())
FPS=24
W,H=1920,1080
ffmpeg=imageio_ffmpeg.get_ffmpeg_exe()
output=OUT/'FreshBasket_UI_Feature_Demo.mp4'
cmd=[ffmpeg,'-y','-hide_banner','-loglevel','warning','-f','rawvideo','-vcodec','rawvideo','-pix_fmt','rgb24','-s',f'{W}x{H}','-r',str(FPS),'-i','-','-an','-c:v','libx264','-preset','veryfast','-crf','21','-threads','2','-pix_fmt','yuv420p','-movflags','+faststart','-metadata','title=FreshBasket — UI & Feature Demonstration','-metadata','comment=Source-based UI previews with sample data. Not a recording of live Windows/ACE operations. Runtime verification pending.',str(output)]
process=subprocess.Popen(cmd,stdin=subprocess.PIPE)
images=[Image.open(WORK/f'scene-{i:02}.png').convert('RGB') for i in range(len(scenes))]
ends=[Image.open(WORK/f'scene-{i:02}-end.png').convert('RGB') if (WORK/f'scene-{i:02}-end.png').exists() else images[i] for i in range(len(scenes))]
def ease(t):
    t=max(0,min(1,t));return t*t*(3-2*t)
def pointer(im,x,y,opacity):
    layer=Image.new('RGBA',(W,H),(0,0,0,0));d=ImageDraw.Draw(layer)
    points=[(x,y),(x+2,y+30),(x+9,y+23),(x+16,y+36),(x+23,y+32),(x+16,y+19),(x+27,y+18)]
    d.polygon(points,fill=(255,255,255,int(255*opacity)),outline=(24,54,35,int(255*opacity)),width=2)
    im.paste(layer,(0,0),layer)
def stamp(seconds):
    ms=round(seconds*1000);h,rem=divmod(ms,3600000);m,rem=divmod(rem,60000);s,ms=divmod(rem,1000)
    return f'{h:02}:{m:02}:{s:02},{ms:03}'
subtitles=[];elapsed=0;total=sum(s['seconds'] for s in scenes)
try:
    for i,s in enumerate(scenes):
        subtitles.append(f'{i+1}\n{stamp(elapsed)} --> {stamp(elapsed+s["seconds"])}\n{s["caption"]}\n')
        duration=s['seconds']
        for f in range(duration*FPS):
            t=f/FPS
            mix=ease((t-duration*.48)/.65) if ends[i] is not images[i] else 0
            frame=Image.blend(images[i],ends[i],mix) if mix else images[i].copy()
            if i and t<.45:
                frame=Image.blend(ends[i-1],frame,ease(t/.45))
            if s.get('pointer') and .6<t<duration-.5:
                start=s['pointer'];end=s.get('pointerEnd',start)
                progress=ease((t-.7)/1.25)
                tx=start['x']+(end['x']-start['x'])*mix
                ty=start['y']+(end['y']-start['y'])*mix
                x=1740+(tx-1740)*progress;y=820+(ty-820)*progress
                opacity=min(1,(t-.6)/.2,(duration-.5-t)/.25)
                pointer(frame,x,y,max(0,opacity))
            draw=ImageDraw.Draw(frame)
            draw.rectangle((80,1072,1840,1076),fill='#294937')
            draw.rectangle((80,1072,80+1760*(elapsed+t)/total,1076),fill='#c1d994')
            process.stdin.write(frame.tobytes())
        elapsed+=duration
        print(f'Encoded scene {i+1}/{len(scenes)} ({elapsed}/{total}s)',flush=True)
finally:
    process.stdin.close()
    rc=process.wait()
if rc:raise SystemExit(f'FFmpeg failed: {rc}')
(OUT/'FreshBasket_UI_Feature_Demo.srt').write_text('\n'.join(subtitles))
print(f'Created {output} ({output.stat().st_size/1024/1024:.1f} MiB)',flush=True)
