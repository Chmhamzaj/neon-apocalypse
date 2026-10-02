package com.hamza.streetsovereign;

import android.content.Context;
import android.content.SharedPreferences;
import android.graphics.*;
import android.graphics.drawable.*;
import android.os.Handler;
import android.os.Looper;
import android.view.*;
import android.view.HapticFeedbackConstants;
import java.util.*;

public class GameView extends View {
    private final Paint p = new Paint(Paint.ANTI_ALIAS_FLAG);
    private final Paint stroke = new Paint(Paint.ANTI_ALIAS_FLAG);
    private final Random rng = new Random(90210);
    private final Handler handler = new Handler(Looper.getMainLooper());
    private final SharedPreferences save;
    private final ArrayList<Entity> entities = new ArrayList<>();
    private final ArrayList<Particle> particles = new ArrayList<>();
    private final ArrayList<Projectile> shots = new ArrayList<>();
    private final ArrayList<FloatingText> texts = new ArrayList<>();
    private final ArrayList<RainDrop> rain = new ArrayList<>();
    private final String[] districts = {"NEON BAY","IRON DOCKS","SUNSET HEIGHTS","OLD TOWN","KING'S MARKET","LIBERTY CORE","MERCURY STRIP"};
    private final String[] missions = {
        "FIRST RUN","HOT MERCH","DOCKSIDE SWEEP","MIDNIGHT RUSH",
        "THE BIG SCORE","KING'S GAMBIT","NO TURNING BACK","CITY ON FIRE",
        "GHOST RUN","LAST EXIT","SOVEREIGN","FINAL ASCENT"
    };
    private final String[] missionDesc = {
        "Drive to the warehouse marker.",
        "Collect the crate and reach the market.",
        "Clear five hostile crews.",
        "Win the cross-town sprint.",
        "Steal the marked vehicle and escape.",
        "Survive a three-star pursuit.",
        "Take down rival enforcers.",
        "Break through the police roadblock.",
        "Ghost through the docks without raising heat.",
        "Reach the safehouse with $12,000.",
        "Defend the sovereign district.",
        "Own the city."
    };

    private int W,H;
    private float px=2450,py=1750,vx=0,vy=0;
    private float health=100, armor=75, stamina=100, heat=0;
    private int cash=2500, ammo=180, weapon=0, mission=0, kills=0, notoriety=0;
    private int wanted=0, day=18, hour=20, minute=25;
    private boolean started=false, paused=false, mapMode=false, inCar=false, shopMode=false, garageMode=false;
    private boolean fire=false, boost=false, up=false, down=false, left=false, right=false;
    private int joystickPointer=-1, firePointer=-1;
    private float jx=0,jy=0;
    private float cameraX,cameraY;
    private float elapsed=0;
    private float spawnTimer=0;
    private String district="NEON BAY";
    private String weather="CLEAR";
    private long lastSave=0;

    private final float WORLD_W=5200, WORLD_H=3600;

    private final Runnable tick = new Runnable(){ public void run(){ invalidate(); handler.postDelayed(this,16); } };

    public GameView(Context c) {
        super(c);
        setFocusable(true);
        setLayerType(View.LAYER_TYPE_HARDWARE,null);
        save=c.getSharedPreferences("street_sovereign",Context.MODE_PRIVATE);
        cash=save.getInt("cash",2500);
        mission=save.getInt("mission",0);
        kills=save.getInt("kills",0);
        notoriety=save.getInt("notoriety",0);
        buildCity();
        handler.post(tick);
    }

    private void buildCity(){
        entities.clear();
        // roads / parked cars / civilians / gangs
        for(int i=0;i<125;i++){
            float x=140+rng.nextFloat()*(WORLD_W-280);
            float y=120+rng.nextFloat()*(WORLD_H-240);
            int type;
            int roll=i%10;
            if(roll<4) type=1; else if(roll<8) type=2; else if(roll==8) type=3; else type=4;
            entities.add(new Entity(x,y,type, rng.nextInt(7), rng.nextBoolean()));
        }
        for(int i=0;i<210;i++){
            float x=80+rng.nextFloat()*(WORLD_W-160);
            float y=80+rng.nextFloat()*(WORLD_H-160);
            if(Math.abs(x-px)<260 && Math.abs(y-py)<220) continue;
            entities.add(new Entity(x,y,0,rng.nextInt(4),rng.nextBoolean()));
        }
        for(int i=0;i<65;i++){
            float x=100+rng.nextFloat()*(WORLD_W-200), y=100+rng.nextFloat()*(WORLD_H-200);
            entities.add(new Entity(x,y,5,rng.nextInt(5),true));
        }
        for(int i=0;i<80;i++) rain.add(new RainDrop(rng.nextFloat()*WORLD_W,rng.nextFloat()*WORLD_H));
    }

    @Override protected void onDraw(Canvas c){
        W=getWidth(); H=getHeight();
        if(!started){ drawMenu(c); return; }
        elapsed+=0.016f;
        updateGame(0.016f);
        float targetX=Math.max(0,Math.min(WORLD_W-W,px-W*0.5f));
        float targetY=Math.max(0,Math.min(WORLD_H-H,py-H*0.5f));
        cameraX += (targetX-cameraX)*0.12f;
        cameraY += (targetY-cameraY)*0.12f;
        if(mapMode){ drawGame(c); drawMap(c); return; }
        drawGame(c);
        drawHUD(c);
        drawControls(c);
        if(shopMode) drawShop(c);
        if(garageMode) drawGarage(c);
        if(paused) drawPause(c);
    }

    private void updateGame(float dt){
        if(paused||mapMode||shopMode||garageMode) return;
        float ax=jx, ay=jy;
        float len=(float)Math.hypot(ax,ay);
        if(len>1){ax/=len;ay/=len;}
        if(right) ax=1; if(left) ax=-1; if(down) ay=1; if(up) ay=-1;
        float speed=inCar?560:330;
        if(boost && stamina>0){speed*=1.75f;stamina=Math.max(0,stamina-30*dt);}else stamina=Math.min(100,stamina+18*dt);
        vx += ax*speed*3.2f*dt; vy += ay*speed*3.2f*dt;
        float damp=inCar?0.84f:0.70f; vx*=damp;vy*=damp;
        float s=(float)Math.hypot(vx,vy); float cap=inCar?690:360; if(s>cap){vx*=cap/s;vy*=cap/s;}
        px=Math.max(40,Math.min(WORLD_W-40,px+vx*dt));
        py=Math.max(40,Math.min(WORLD_H-40,py+vy*dt));
        if(fire) shoot();
        updateEntities(dt);
        updateShots(dt);
        updateParticles(dt);
        updateTexts(dt);
        updateClock(dt);
        spawnTimer+=dt;
        if(wanted>0 && spawnTimer>2.4f){spawnTimer=0; spawnCop();}
        heat-=dt*(wanted>0?1.6f:4.6f);
        if(heat<0) heat=0;
        wanted=Math.min(5,Math.max(0,(int)Math.ceil(heat/22f)));
        if(health<=0){respawn();}
        checkMission();
        updateDistrict();
        if(weather.equals("RAIN")) for(RainDrop r:rain){r.x=(r.x+20*dt)%WORLD_W;r.y=(r.y+520*dt)%WORLD_H;}
        if(System.currentTimeMillis()-lastSave>7000){saveGame();lastSave=System.currentTimeMillis();}
    }

    private void updateEntities(float dt){
        for(Entity e:entities){
            if(e.dead){e.respawn-=dt;continue;}
            if(e.type==0||e.type==2||e.type==5){
                if(e.hostile && dist(e.x,e.y,px,py)<950){float dx=px-e.x,dy=py-e.y,d=Math.max(1,(float)Math.hypot(dx,dy));e.x+=dx/d*(45+e.type*10)*dt;e.y+=dy/d*(45+e.type*10)*dt;}
                else {e.x+=Math.cos(e.a)*e.spd*dt;e.y+=Math.sin(e.a)*e.spd*dt;}
                if(rng.nextFloat()<0.01f*dt)e.a+=(rng.nextFloat()-.5f)*2f;
            } else if(e.type==1||e.type==3||e.type==4){
                e.a+=(rng.nextFloat()-.5f)*dt*3;
                e.x+=Math.cos(e.a)*e.spd*dt*1.7f;e.y+=Math.sin(e.a)*e.spd*dt*1.7f;
            }
            e.x=Math.max(30,Math.min(WORLD_W-30,e.x));e.y=Math.max(30,Math.min(WORLD_H-30,e.y));
            if(e.hostile && dist(e.x,e.y,px,py)<75){health-=4*dt; if(armor>0) armor=Math.max(0,armor-2*dt);}
        }
        for(Iterator<Entity> it=entities.iterator();it.hasNext();){
            Entity e=it.next();
            if(e.dead && e.respawn<=0){e.dead=false;e.hp=100;e.x=100+rng.nextFloat()*(WORLD_W-200);e.y=100+rng.nextFloat()*(WORLD_H-200);}
        }
    }

    private void updateShots(float dt){
        for(Iterator<Projectile> it=shots.iterator();it.hasNext();){
            Projectile q=it.next();q.x+=q.dx*dt;q.y+=q.dy*dt;q.life-=dt;
            if(q.life<=0){explode(q.x,q.y,8,false);it.remove();continue;}
            for(Entity e:entities){
                if(e.dead||!e.hostile) continue;
                if(dist(q.x,q.y,e.x,e.y)<26){
                    e.hp-=q.damage;explode(e.x,e.y,12,true);
                    if(e.hp<=0){e.dead=true;e.respawn=6;cash+=100+weapon*30;kills++;notoriety++;addText(e.x,e.y,"+$"+(100+weapon*30));}
                    it.remove();break;
                }
            }
            if(q.x<0||q.y<0||q.x>WORLD_W||q.y>WORLD_H)it.remove();
        }
    }

    private void updateParticles(float dt){for(Iterator<Particle> it=particles.iterator();it.hasNext();){Particle a=it.next();a.life-=dt;a.x+=a.dx*dt;a.y+=a.dy*dt;a.dy+=120*dt;if(a.life<=0)it.remove();}}
    private void updateTexts(float dt){for(Iterator<FloatingText> it=texts.iterator();it.hasNext();){FloatingText t=it.next();t.life-=dt;t.y-=28*dt;if(t.life<=0)it.remove();}}
    private void updateClock(float dt){
        minute += (int)(dt*9);
        if(minute>=60){minute-=60;hour++;} if(hour>=24){hour=0;day++;}
        if(hour>=6 && hour<11) weather="CLEAR";
        else if(hour>=11 && hour<17) weather=(day%3==0?"HAZE":"CLEAR");
        else if(hour>=17 && hour<22) weather=(day%4==0?"RAIN":"SUNSET");
        else weather=(day%5==0?"RAIN":"NIGHT");
    }
    private void updateDistrict(){
        int d=(int)((px/WORLD_W)*districts.length); if(d<0)d=0;if(d>=districts.length)d=districts.length-1;district=districts[d];
    }

    private void shoot(){
        if(ammo<=0){addText(px,py-60,"OUT OF AMMO");fire=false;return;}
        if(rng.nextFloat()>0.32f)return;
        ammo--;
        float tx=px+(jx==0?1:jx)*120, ty=py+(jy==0?0:jy)*120;
        Entity best=null;float bd=800;
        for(Entity e:entities)if(!e.dead&&e.hostile){float d=dist(px,py,e.x,e.y);if(d<bd){bd=d;best=e;}}
        if(best!=null){tx=best.x;ty=best.y;}
        float dx=tx-px,dy=ty-py,l=(float)Math.max(1,Math.hypot(dx,dy));
        float sp=1050+(weapon*170);
        shots.add(new Projectile(px+dx/l*26,py+dy/l*26,dx/l*sp,dy/l*sp,0.7f,22+weapon*10));
        heat=Math.min(100,heat+7);wanted=Math.min(5,(int)Math.ceil(heat/22f));
        vibrate();
    }
    private void spawnCop(){entities.add(new Entity(px+450+rng.nextFloat()*260,py+250+rng.nextFloat()*220,6,2,true));addText(px+40,py-100,"POLICE INBOUND");}
    private void explode(float x,float y,int n,boolean hit){
        for(int i=0;i<n;i++){double a=rng.nextDouble()*Math.PI*2;float sp=40+rng.nextFloat()*180;particles.add(new Particle(x,y,(float)Math.cos(a)*sp,(float)Math.sin(a)*sp,0.35f+(rng.nextFloat()*.6f),hit));}
    }
    private void checkMission(){
        switch(mission){
            case 0: if(dist(px,py,4300,500)<150){complete(500);mission=1;}break;
            case 1: if(dist(px,py,700,3000)<160){complete(800);mission=2;}break;
            case 2: if(kills>=5){complete(1100);mission=3;}break;
            case 3: if(inCar&&dist(px,py,4550,2700)<180){complete(1300);mission=4;}break;
            case 4: if(inCar&&wanted>=2&&cash>5000){complete(1800);mission=5;}break;
            case 5: if(wanted>=3&&heat>65&&health>45){complete(2400);mission=6;}break;
            case 6: if(kills>=12){complete(3100);mission=7;}break;
            case 7: if(dist(px,py,1100,650)<160&&wanted>=4){complete(4000);mission=8;}break;
            case 8: if(wanted==0&&heat<5&&dist(px,py,800,2900)<190){complete(4600);mission=9;}break;
            case 9: if(cash>=12000&&dist(px,py,2800,2600)<170){complete(6000);mission=10;}break;
            case 10: if(kills>=24&&wanted>=2){complete(8000);mission=11;}break;
            case 11: if(cash>=30000&&kills>=40){complete(15000);mission=12;}break;
        }
        if(mission>=12){mission=11;}
    }
    private void complete(int reward){cash+=reward;notoriety+=2;addText(px,py-130,"MISSION COMPLETE +$"+reward);saveGame();}
    private void respawn(){health=100;armor=65;cash=Math.max(0,cash-800);wanted=0;heat=0;px=2450;py=1750;inCar=false;addText(px,py-120,"HOSPITAL BILL -$800");}
    private void updateDistrictDummy(){}

    private void drawMenu(Canvas c){
        p.setShader(new LinearGradient(0,0,W,H,Color.rgb(5,8,18),Color.rgb(56,17,65),Shader.TileMode.CLAMP));
        c.drawRect(0,0,W,H,p);p.setShader(null);
        // skyline
        for(int i=0;i<34;i++){
            float bw=35+rng.nextInt(65), bh=70+rng.nextInt(Math.max(80,H/3));
            float x=(i*75)%W, y=H*0.50f-bh;
            p.setColor(Color.rgb(15+i%4*8,20+i%5*7,28+i%3*9));c.drawRect(x,y,x+bw,H*.72f,p);
            p.setColor(Color.argb(150,255,196,77));
            for(int xx=0;xx<(int)bw/14;xx++)for(int yy=0;yy<(int)bh/22;yy++) if((xx+yy+i)%3==0)c.drawRect(x+7+xx*14,y+8+yy*22,x+12+xx*14,y+13+yy*22,p);
        }
        // car
        p.setColor(Color.rgb(206,46,52));c.drawRoundRect(W*.46f,H*.61f,W*.78f,H*.74f,28,28,p);p.setColor(Color.rgb(24,26,31));c.drawRoundRect(W*.55f,H*.59f,W*.69f,H*.68f,18,18,p);
        text(c,"STREET",W*.07f,H*.27f,76,Color.WHITE);text(c,"SOVEREIGN",W*.07f,H*.37f,80,Color.WHITE);
        text(c,"HAMZA",W*.17f,H*.46f,88,Color.rgb(255,56,48));text(c,"THREE CITIES • ONE EMPIRE • YOUR RULES",W*.08f,H*.52f,22,Color.WHITE);
        button(c,W*.12f,H*.66f,W*.34f,H*.78f,"PLAY CAMPAIGN",Color.WHITE,Color.BLACK);
        button(c,W*.38f,H*.66f,W*.60f,H*.78f,"CITY MAP",Color.rgb(255,197,69),Color.BLACK);
        button(c,W*.64f,H*.66f,W*.88f,H*.78f,"GARAGE",Color.rgb(64,170,255),Color.WHITE);
        text(c,"OFFLINE • ANDROID • ORIGINAL IP",W*.07f,H*.9f,18,Color.LTGRAY);
        text(c,"v2.0  |  HIGH-DETAIL MOBILE EDITION",W*.68f,H*.9f,16,Color.LTGRAY);
    }

    private void drawGame(Canvas c){
        c.drawColor(Color.rgb(25,33,28));
        c.save(); c.translate(-cameraX,-cameraY);
        drawTerrain(c);
        drawRoads(c);
        drawDistrictArt(c);
        drawMissionMarker(c);
        drawEntities(c);
        drawShots(c);
        drawParticles(c);
        drawPlayer(c);
        drawRain(c);
        drawTimeTint(c);
        c.restore();
    }
    private void drawTerrain(Canvas c){
        p.setShader(null);p.setStyle(Paint.Style.FILL);
        p.setColor(Color.rgb(47,71,54));c.drawRect(0,0,WORLD_W,WORLD_H,p);
        // water
        p.setColor(Color.rgb(18,56,82));c.drawRect(0,0,WORLD_W,280,p);c.drawRect(0,WORLD_H-240,WORLD_W,WORLD_H,p);
        // blocks
        for(int x=80;x<WORLD_W;x+=320)for(int y=330;y<WORLD_H-280;y+=300){
            int t=(x/320+y/300)%4;
            int col=new int[]{Color.rgb(63,70,76),Color.rgb(90,72,62),Color.rgb(70,77,83),Color.rgb(77,66,88)}[t];
            p.setColor(col);c.drawRect(x,y,x+230,y+205,p);
            p.setColor(Color.argb(155,235,218,165));
            for(int wx=0;wx<4;wx++)for(int wy=0;wy<4;wy++)if((wx+wy+t)%2==0)c.drawRect(x+16+wx*48,y+14+wy*42,x+31+wx*48,y+28+wy*42,p);
        }
        // parks
        p.setColor(Color.rgb(41,104,58));c.drawCircle(900,780,250,p);c.drawCircle(3750,800,310,p);c.drawCircle(4350,2900,260,p);
        for(int i=0;i<38;i++){float a=rng.nextFloat()*(float)Math.PI*2,r=60+rng.nextFloat()*230;float x=900+(float)Math.cos(a)*r,y=780+(float)Math.sin(a)*r;p.setColor(Color.rgb(32,78,42));c.drawCircle(x,y,16,p);}
    }
    private void drawRoads(Canvas c){
        p.setColor(Color.rgb(33,35,39));
        for(int x=100;x<WORLD_W;x+=420)c.drawRect(x,280,x+92,WORLD_H-240,p);
        for(int y=360;y<WORLD_H-280;y+=310)c.drawRect(0,y,WORLD_W,y+86,p);
        p.setColor(Color.rgb(208,179,75));
        for(int x=110;x<WORLD_W;x+=420)c.drawRect(x+40,300,x+43,WORLD_H-250,p);
        for(int y=390;y<WORLD_H-260;y+=310)c.drawRect(0,y+42,WORLD_W,y+45,p);
        // bridges
        p.setColor(Color.rgb(72,69,64));c.drawRect(2040,280,2460,WORLD_H-240,p);
        p.setColor(Color.rgb(126,112,92));for(int y=340;y<WORLD_H-260;y+=48)c.drawRect(2040,y,2460,y+5,p);
    }
    private void drawDistrictArt(Canvas c){
        text(c,"NEON BAY",150,330,38,Color.WHITE);text(c,"IRON DOCKS",1650,330,38,Color.WHITE);
        text(c,"SUNSET HEIGHTS",3550,330,38,Color.WHITE);text(c,"LIBERTY CORE",4050,WORLD_H-290,38,Color.WHITE);
    }
    private void drawMissionMarker(Canvas c){
        float tx,ty;
        switch(mission){
            case 0:tx=4300;ty=500;break;case 1:tx=700;ty=3000;break;case 2:tx=3000;ty=1500;break;case 3:tx=4550;ty=2700;break;
            case 4:tx=2100;ty=3000;break;case 5:tx=1400;ty=2000;break;case 6:tx=3500;ty=1900;break;case 7:tx=1100;ty=650;break;
            case 8:tx=800;ty=2900;break;case 9:tx=2800;ty=2600;break;case 10:tx=1800;ty=800;break;default:tx=4200;ty=420;break;
        }
        p.setStyle(Paint.Style.STROKE);p.setStrokeWidth(7);p.setColor(Color.WHITE);c.drawCircle(tx,ty,55+(float)Math.sin(elapsed*4)*8,p);p.setStyle(Paint.Style.FILL);text(c,"M",tx-12,ty+14,36,Color.WHITE);
    }
    private void drawEntities(Canvas c){
        for(Entity e:entities){if(e.dead)continue;switch(e.type){
            case 0: drawPed(c,e);break;case 1:drawCar(c,e,Color.rgb(204,57,67));break;case 2:drawPed(c,e);break;case 3:drawCar(c,e,Color.rgb(53,116,216));break;case 4:drawCar(c,e,Color.rgb(220,175,61));break;
            case 5:drawPed(c,e);break;case 6:drawPolice(c,e);break;}}
    }
    private void drawPed(Canvas c,Entity e){p.setColor(e.hostile?Color.rgb(155,48,55):Color.rgb(205,160,105));c.drawCircle(e.x,e.y,20,p);p.setColor(Color.rgb(28,31,36));c.drawCircle(e.x,e.y-7,7,p);p.setColor(Color.WHITE);c.drawRect(e.x-7,e.y+7,e.x+7,e.y+21,p);}
    private void drawCar(Canvas c,Entity e,int color){p.setColor(color);c.drawRoundRect(e.x-48,e.y-25,e.x+48,e.y+25,12,12,p);p.setColor(Color.rgb(25,28,34));c.drawRoundRect(e.x-24,e.y-18,e.x+24,e.y+18,9,9,p);p.setColor(Color.WHITE);c.drawCircle(e.x-32,e.y-24,7,p);c.drawCircle(e.x+32,e.y-24,7,p);c.drawCircle(e.x-32,e.y+24,7,p);c.drawCircle(e.x+32,e.y+24,7,p);}
    private void drawPolice(Canvas c,Entity e){drawCar(c,e,Color.rgb(45,88,188));p.setColor(Color.rgb(220,55,58));c.drawRect(e.x-15,e.y-29,e.x,e.y-24,p);p.setColor(Color.WHITE);c.drawRect(e.x,e.y-29,e.x+15,e.y-24,p);}
    private void drawShots(Canvas c){p.setColor(Color.WHITE);for(Projectile q:shots)c.drawCircle(q.x,q.y,5,p);}
    private void drawParticles(Canvas c){for(Particle a:particles){p.setColor(a.hit?Color.rgb(255,86,47):Color.rgb(255,204,80));p.setAlpha((int)(255*Math.max(0,Math.min(1,a.life/0.9f))));c.drawCircle(a.x,a.y,5+a.life*9,p);p.setAlpha(255);}}
    private void drawPlayer(Canvas c){
        if(inCar){p.setColor(Color.rgb(22,23,28));c.drawRoundRect(px-58,py-30,px+58,py+30,15,15,p);p.setColor(Color.rgb(255,68,54));c.drawRoundRect(px-28,py-22,px+28,py+22,9,9,p);}
        else {p.setColor(Color.WHITE);c.drawCircle(px,py,24,p);p.setColor(Color.rgb(25,29,35));c.drawCircle(px,py-8,8,p);p.setColor(Color.rgb(216,61,62));c.drawRect(px-10,py+8,px+10,py+23,p);}
    }
    private void drawRain(Canvas c){if(!weather.equals("RAIN"))return;p.setColor(Color.argb(100,175,205,255));p.setStrokeWidth(2);for(RainDrop r:rain)c.drawLine(r.x,r.y,r.x+7,r.y+22,p);}
    private void drawTimeTint(Canvas c){
        int alpha=0;
        if(hour>=20||hour<6)alpha=105; else if(hour>=17)alpha=55;
        if(weather.equals("HAZE"))alpha+=25;
        if(alpha>0){p.setColor(Color.argb(alpha,8,18,42));c.drawRect(cameraX,cameraY,cameraX+W,cameraY+H,p);}
        if(weather.equals("SUNSET")){p.setColor(Color.argb(55,255,94,42));c.drawRect(cameraX,cameraY,cameraX+W,cameraY+H,p);}
    }
    private void drawHUD(Canvas c){
        panel(c,18,16,455,148,Color.argb(195,0,0,0));
        text(c,"HAMZA",35,48,22,Color.WHITE);text(c,"$"+cash,35,80,30,Color.WHITE);text(c,district,35,112,17,Color.LTGRAY);
        bar(c,185,62,420,80,health,100,Color.rgb(75,210,115));text(c,"HP",188,56,14,Color.WHITE);
        bar(c,185,92,420,110,armor,100,Color.rgb(70,150,245));text(c,"ARM",188,87,14,Color.WHITE);
        text(c,"WPN "+new String[]{"PISTOL","SMG","SHOTGUN","MAGNUM"}[weapon],35,140,16,Color.WHITE);text(c,"AMMO "+ammo,180,140,16,Color.WHITE);
        if(wanted>0){text(c,"WANTED",W-250,36,17,Color.WHITE);text(c,"★★★★★".substring(0,wanted),W-250,70,30,Color.rgb(255,205,70));}
        text(c,"DAY "+day+"  "+String.format(Locale.US,"%02d:%02d",hour,minute),W-250,102,16,Color.WHITE);
        // mission card
        panel(c,W*.34f,12,W*.66f,102,Color.argb(155,0,0,0));
        text(c,mission<12?missions[mission]:"THE END",W*.36f,40,20,Color.WHITE);
        text(c,missionDesc[Math.min(mission,11)],W*.36f,72,15,Color.LTGRAY);
        text(c,"NOTORIETY "+notoriety,W*.36f,94,14,Color.rgb(255,195,80));
        // minimap
        float mw=220,mh=150,mx=W-mw-18,my=H-mh-18;panel(c,mx,my,mx+mw,my+mh,Color.argb(185,0,0,0));
        p.setColor(Color.rgb(53,61,66));c.drawRect(mx+8,my+8,mx+mw-8,my+mh-8,p);
        for(int i=0;i<7;i++){p.setColor(Color.argb(110,255,255,255));c.drawRect(mx+15+(i*27)%175,my+10,mx+18+(i*27)%175,my+mh-12,p);}
        float qx=mx+10+(px/WORLD_W)*(mw-20), qy=my+10+(py/WORLD_H)*(mh-20);p.setColor(Color.WHITE);c.drawCircle(qx,qy,6,p);
        text(c,"MISSION "+Math.min(mission+1,12)+"/12",mx+12,my+mh-12,14,Color.WHITE);
        for(FloatingText t:texts){text(c,t.s,t.x-cameraX,t.y-cameraY,18,Color.WHITE);}
    }
    private void drawControls(Canvas c){
        float cx=118,cy=H-120;p.setColor(Color.argb(70,255,255,255));c.drawCircle(cx,cy,86,p);p.setColor(Color.argb(150,255,255,255));c.drawCircle(cx+jx*42,cy+jy*42,32,p);
        roundButton(c,W-135,H-108,76,"FIRE");roundButton(c,W-275,H-105,62,inCar?"EXIT":"CAR");roundButton(c,W-405,H-105,55,boost?"NITRO":"BOOST");
        roundButton(c,W-135,125,58,"MAP");roundButton(c,W-275,125,58,"SHOP");roundButton(c,W-405,125,58,"PAUSE");
    }
    private void drawMap(Canvas c){
        p.setColor(Color.argb(245,8,10,15));c.drawRect(0,0,W,H,p);text(c,"CITY MAP",50,60,38,Color.WHITE);
        float mw=W*.72f,mh=H*.68f,mx=W*.14f,my=H*.17f;panel(c,mx,my,mx+mw,my+mh,Color.rgb(30,39,45));
        p.setColor(Color.rgb(48,76,57));c.drawRect(mx+18,my+18,mx+mw-18,my+mh-18,p);
        for(int i=0;i<12;i++){p.setColor(Color.rgb(43,47,51));float xx=mx+50+i*mw/12f;c.drawRect(xx,my+20,xx+8,my+mh-20,p);}
        for(int i=0;i<9;i++){p.setColor(Color.rgb(43,47,51));float yy=my+42+i*mh/10f;c.drawRect(mx+20,yy,mx+mw-20,yy+8,p);}
        p.setColor(Color.WHITE);c.drawCircle(mx+20+(px/WORLD_W)*(mw-40),my+20+(py/WORLD_H)*(mh-40),9,p);
        text(c,"X CLOSE",W*.80f,H*.88f,18,Color.WHITE);
        text(c,"FAST TRAVEL LOCKED • COMPLETE MORE MISSIONS",W*.24f,H*.92f,17,Color.LTGRAY);
    }
    private void drawShop(Canvas c){
        p.setColor(Color.argb(230,8,10,15));c.drawRect(W*.18f,H*.15f,W*.82f,H*.86f,p);
        text(c,"ARMORY • STREET MARKET",W*.23f,H*.23f,30,Color.WHITE);
        text(c,"1  PISTOL      $0         OWNED",W*.25f,H*.34f,21,Color.WHITE);
        text(c,"2  SMG       $4,500       "+(cash>=4500?"BUY":"LOCKED"),W*.25f,H*.43f,21,Color.WHITE);
        text(c,"3  SHOTGUN   $7,500       "+(cash>=7500?"BUY":"LOCKED"),W*.25f,H*.52f,21,Color.WHITE);
        text(c,"4  MAGNUM   $12,000       "+(cash>=12000?"BUY":"LOCKED"),W*.25f,H*.61f,21,Color.WHITE);
        text(c,"TAP A LINE TO EQUIP / BUY",W*.25f,H*.73f,16,Color.LTGRAY);
    }
    private void drawGarage(Canvas c){
        p.setColor(Color.argb(235,9,12,18));c.drawRect(W*.14f,H*.12f,W*.86f,H*.88f,p);
        text(c,"GARAGE • SPEED LAB",W*.20f,H*.22f,32,Color.WHITE);
        // car art
        p.setColor(Color.rgb(200,52,65));c.drawRoundRect(W*.30f,H*.38f,W*.70f,H*.56f,24,24,p);p.setColor(Color.rgb(24,27,33));c.drawRoundRect(W*.40f,H*.34f,W*.60f,H*.46f,16,16,p);
        text(c,"ENGINE",W*.22f,H*.66f,18,Color.LTGRAY);text(c,"NITRO",W*.44f,H*.66f,18,Color.LTGRAY);text(c,"ARMOR",W*.64f,H*.66f,18,Color.LTGRAY);
        text(c,"UPGRADES UNLOCKED BY NOTORIETY",W*.28f,H*.76f,17,Color.WHITE);
    }
    private void drawPause(Canvas c){
        p.setColor(Color.argb(190,0,0,0));c.drawRect(0,0,W,H,p);text(c,"PAUSED",W*.42f,H*.37f,54,Color.WHITE);
        text(c,"TAP PAUSE TO RESUME",W*.34f,H*.49f,21,Color.LTGRAY);
        text(c,"PROGRESS SAVED AUTOMATICALLY",W*.30f,H*.56f,18,Color.LTGRAY);
    }

    private void panel(Canvas c,float l,float t,float r,float b,int color){p.setColor(color);p.setStyle(Paint.Style.FILL);c.drawRoundRect(l,t,r,b,18,18,p);}
    private void bar(Canvas c,float l,float t,float r,float b,float val,float max,int color){p.setColor(Color.rgb(60,60,63));c.drawRoundRect(l,t,r,b,9,9,p);p.setColor(color);c.drawRoundRect(l,t,l+(r-l)*Math.max(0,Math.min(1,val/max)),b,9,9,p);}
    private void button(Canvas c,float l,float t,float r,float b,String s,int bg,int fg){p.setColor(bg);c.drawRoundRect(l,t,r,b,18,18,p);text(c,s,(l+r)/2-p.measureText(s)/2,(t+b)/2+8,23,fg);}
    private void roundButton(Canvas c,float x,float y,float radius,String s){p.setColor(Color.argb(110,255,255,255));c.drawCircle(x,y,radius,p);text(c,s,x-p.measureText(s)/2,y+6,15,Color.WHITE);}
    private void text(Canvas c,String s,float x,float y,float size,int color){p.setTypeface(Typeface.create(Typeface.DEFAULT,Typeface.BOLD));p.setTextSize(size);p.setColor(color);p.setStyle(Paint.Style.FILL);c.drawText(s,x,y,p);}
    private float dist(float ax,float ay,float bx,float by){return (float)Math.hypot(ax-bx,ay-by);}
    private void addText(float x,float y,String s){texts.add(new FloatingText(x,y,s));}
    private void saveGame(){save.edit().putInt("cash",cash).putInt("mission",mission).putInt("kills",kills).putInt("notoriety",notoriety).apply();}
    private void vibrate(){performHapticFeedback(HapticFeedbackConstants.KEYBOARD_TAP,HapticFeedbackConstants.FLAG_IGNORE_GLOBAL_SETTING);}

    private void toggleCar(){
        if(inCar){inCar=false;vx*=0.2f;vy*=0.2f;addText(px,py-60,"ON FOOT");return;}
        Entity best=null;float bd=105;for(Entity e:entities)if(!e.dead&&(e.type==1||e.type==3||e.type==4)){float d=dist(px,py,e.x,e.y);if(d<bd){bd=d;best=e;}}
        if(best!=null){inCar=true;px=best.x;py=best.y;best.dead=true;best.respawn=9;addText(px,py-60,"VEHICLE HIJACKED");}else addText(px,py-60,"GET CLOSER TO A CAR");
    }

    @Override public boolean onTouchEvent(MotionEvent e){
        float x=e.getX(),y=e.getY();int action=e.getActionMasked();
        if(action==MotionEvent.ACTION_DOWN){
            if(!started){if(y>H*.60f&&y<H*.82f&&x>W*.10f&&x<W*.40f){started=true;return true;}return true;}
            if(shopMode){if(x>W*.20f&&x<W*.82f&&y>H*.28f&&y<H*.66f){int row=(int)((y-H*.28f)/(H*.10f));buyWeapon(Math.min(3,Math.max(0,row)));}else shopMode=false;return true;}
            if(garageMode){garageMode=false;return true;}
            if(paused){if(x>W*.35f&&x<W*.65f&&y>H*.38f&&y<H*.62f)paused=false;return true;}
            if(mapMode){if(x>W*.70f&&y>H*.82f)mapMode=false;return true;}
            if(x>W-200&&y<220){mapMode=true;return true;}
            if(x>W-340&&x<W-210&&y<220){shopMode=true;return true;}
            if(x>W-470&&x<W-350&&y<220){paused=true;return true;}
            if(x>W-205&&y>H-185){fire=true;firePointer=e.getPointerId(0);}
            else if(x>W-335&&x<W-215&&y>H-180){toggleCar();}
            else if(x>W-470&&x<W-345&&y>H-180){boost=true;}
            else if(x<260&&y>H-250){joystickPointer=e.getPointerId(0);setJoy(x,y);}
            return true;
        }
        if(action==MotionEvent.ACTION_MOVE){if(joystickPointer>=0&&e.getPointerId(e.getActionIndex())==joystickPointer)setJoy(x,y);return true;}
        if(action==MotionEvent.ACTION_UP||action==MotionEvent.ACTION_CANCEL){
            if(e.getPointerId(e.getActionIndex())==firePointer){fire=false;firePointer=-1;}
            if(e.getPointerId(e.getActionIndex())==joystickPointer){joystickPointer=-1;jx=jy=0;}
            boost=false;return true;
        }
        return true;
    }
    private void setJoy(float x,float y){float cx=118,cy=H-120;jx=(x-cx)/86f;jy=(y-cy)/86f;float l=(float)Math.hypot(jx,jy);if(l>1){jx/=l;jy/=l;}}
    private void buyWeapon(int w){
        int[] cost={0,4500,7500,12000};if(cash>=cost[w]){cash-=cost[w];weapon=w;ammo+=120+w*50;shopMode=false;addText(px,py-70,"EQUIPPED "+new String[]{"PISTOL","SMG","SHOTGUN","MAGNUM"}[w]);saveGame();}
    }

    static class Entity{
        float x,y,a=(float)(Math.random()*6.28),spd=18+((int)(Math.random()*35)),hp=100,respawn=6;
        int type,skin;boolean hostile,dead;
        Entity(float x,float y,int type,int skin,boolean hostile){this.x=x;this.y=y;this.type=type;this.skin=skin;this.hostile=hostile;if(type==6){this.spd=80;this.hp=130;}}
    }
    static class Projectile{float x,y,dx,dy,life,damage;Projectile(float x,float y,float dx,float dy,float life,float damage){this.x=x;this.y=y;this.dx=dx;this.dy=dy;this.life=life;this.damage=damage;}}
    static class Particle{float x,y,dx,dy,life;boolean hit;Particle(float x,float y,float dx,float dy,float life,boolean hit){this.x=x;this.y=y;this.dx=dx;this.dy=dy;this.life=life;this.hit=hit;}}
    static class FloatingText{float x,y,life=1.4f;String s;FloatingText(float x,float y,String s){this.x=x;this.y=y;this.s=s;}}
    static class RainDrop{float x,y;RainDrop(float x,float y){this.x=x;this.y=y;}}
}
