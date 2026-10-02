package com.hamza.streetsovereign;

import android.content.Context;
import android.graphics.*;
import android.view.*;
import java.util.*;

public class GameView extends SurfaceView implements Runnable, SurfaceHolder.Callback {
    private final Paint p = new Paint(3);
    private final Random rng = new Random(77);
    private Thread loop; private boolean running;
    private long last;
    private int W,H;
    private boolean started=false, paused=false, firing=false;
    private float joyX=0, joyY=0; private int joyPointer=-1;
    private int firePointer=-1, enterPointer=-1, brakePointer=-1;
    private float playerX=1450, playerY=1180, vx=0, vy=0;
    private float health=100, stamina=100; private int cash=2500;
    private int wanted=0; private float heat=0;
    private boolean inCar=false; private int carIndex=-1;
    private final ArrayList<Car> cars=new ArrayList<>();
    private final ArrayList<Npc> npcs=new ArrayList<>();
    private final ArrayList<Npc> cops=new ArrayList<>();
    private final ArrayList<Bld> buildings=new ArrayList<>();
    private final ArrayList<Shot> shots=new ArrayList<>();
    private final ArrayList<String> feed=new ArrayList<>();
    private int mission=0; private float missionTimer=0; private int missionKills=0;
    private final float WORLD_W=3200, WORLD_H=2400;
    private final String[] names={"THE FIRST RUN","HOT MERCH","DOCKSIDE PROBLEM","CROSS-TOWN","NIGHT SHIFT","HEAT CHECK","THE FINAL SCORE"};

    public GameView(Context c){ super(c); setFocusable(true); getHolder().addCallback(this); buildWorld(); }

    void buildWorld(){
        cars.clear(); npcs.clear(); cops.clear(); buildings.clear();
        for(int i=0;i<52;i++){
            boolean vert=(i%2==0); float x=vert?520+(i%7)*430:280+(i%8)*390; float y=vert?250+(i%6)*350:350+(i%6)*330;
            cars.add(new Car(x,y, i%5, i%3==0));
        }
        for(int i=0;i<70;i++){
            npcs.add(new Npc(220+rng.nextFloat()*2760, 220+rng.nextFloat()*1900, i%9==0));
        }
        for(int i=0;i<145;i++){
            float x=120+rng.nextFloat()*2960, y=140+rng.nextFloat()*2120;
            if(Math.abs(x-1450)<430 && Math.abs(y-1180)<300) continue;
            float w=50+rng.nextFloat()*150, h=50+rng.nextFloat()*130;
            if((x<950 && y<750) || (x>2200 && y>1550)) { w*=1.4f; h*=1.2f; }
            buildings.add(new Bld(x,y,w,h, i%4));
        }
        addFeed("WELCOME TO STREET SOVEREIGN");
        addFeed("A NEW CITY. YOUR RULES.");
    }

    void addFeed(String s){ feed.add(0,s); while(feed.size()>4) feed.remove(feed.size()-1); }

    @Override public void run(){
        last=System.nanoTime();
        while(running){
            long now=System.nanoTime(); float dt=Math.min(0.033f,(now-last)/1e9f); last=now;
            if(started && !paused) update(dt);
            postInvalidate();
            try{ Thread.sleep(12); }catch(Exception ignored){}
        }
    }

    void update(float dt){
        missionTimer += dt;
        if(health<=0){ health=100; playerX=1450; playerY=1180; inCar=false; wanted=0; addFeed("YOU SURVIVED. KEEP MOVING."); }

        float mx=joyX, my=joyY;
        float len=(float)Math.hypot(mx,my); if(len>1){mx/=len;my/=len;}
        if(inCar){
            float accel=mx*1800f*dt; vx += accel; vy += my*1800f*dt;
            vx*=0.92f; vy*=0.92f;
            if(brakePointer>=0){vx*=0.78f;vy*=0.78f;}
            float speed=(float)Math.hypot(vx,vy); if(speed>720){vx*=720/speed;vy*=720/speed;}
            playerX += vx*dt; playerY += vy*dt;
            playerX=Math.max(40,Math.min(WORLD_W-40,playerX)); playerY=Math.max(40,Math.min(WORLD_H-40,playerY));
            if(carIndex>=0){cars.get(carIndex).x=playerX;cars.get(carIndex).y=playerY;}
        } else {
            float sp=300*dt; playerX += mx*sp; playerY += my*sp;
            playerX=Math.max(30,Math.min(WORLD_W-30,playerX)); playerY=Math.max(30,Math.min(WORLD_H-30,playerY));
        }
        for(Npc n:npcs) n.update(dt);
        for(Npc n:cops) n.updateCop(dt);
        if(firing) shoot();
        for(Iterator<Shot> it=shots.iterator();it.hasNext();){ Shot s=it.next(); s.x+=s.dx*dt;s.y+=s.dy*dt;s.life-=dt; if(s.life<=0){it.remove();continue;}
            for(Npc n:npcs){ if(n.dead)continue; if(dist(s.x,s.y,n.x,n.y)<24){n.hp-=45;it.remove();heat=Math.min(100,heat+13);wanted=Math.min(5,Math.max(wanted,1));if(n.hp<=0){n.dead=true;cash+=120;missionKills++;addFeed("TARGET DOWN +$120");}break;}}
            for(Npc n:cops){ if(n.dead)continue; if(dist(s.x,s.y,n.x,n.y)<24){n.hp-=55;it.remove();heat=100;wanted=Math.min(5,Math.max(wanted,2));if(n.hp<=0){n.dead=true;cash+=250;missionKills++;addFeed("COP DOWN +$250");}break;}}
        }
        npcs.removeIf(n->n.dead && n.respawn<0);
        cops.removeIf(n->n.dead && n.respawn<0);
        for(Npc n:npcs) if(n.dead){n.respawn-=dt;if(n.respawn<=0){n.dead=false;n.hp=100;n.x=rng.nextFloat()*WORLD_W;n.y=rng.nextFloat()*WORLD_H;n.respawn=8;}}
        for(Npc n:cops) if(n.dead){n.respawn-=dt;if(n.respawn<=0){n.dead=false;n.hp=100;n.x=playerX+350+rng.nextFloat()*250;n.y=playerY+350+rng.nextFloat()*250;n.respawn=6;}}
        heat-=dt*(wanted>0?2.1f:5f);
        if(heat<0){heat=0;if(wanted>0 && missionTimer%9<dt && !firing){wanted--;}}
        if(wanted>0 && cops.size()<wanted*4) cops.add(new Npc(playerX+500+rng.nextFloat()*250,playerY+300+rng.nextFloat()*250,true));
        for(Npc c:cops) if(!c.dead && dist(c.x,c.y,playerX,playerY)<60){health-=18*dt;}
        updateMission(dt);
    }

    void updateMission(float dt){
        if(mission==0 && dist(playerX,playerY,2450,450)<100){ mission=1;cash+=500;addFeed("MISSION COMPLETE +$500"); }
        else if(mission==1 && dist(playerX,playerY,700,1800)<110){mission=2;cash+=700;addFeed("HOT MERCH DELIVERED +$700");}
        else if(mission==2 && missionKills>=5){mission=3;cash+=1000;addFeed("DOCKSIDE PROBLEM SOLVED +$1000");}
        else if(mission==3 && inCar && dist(playerX,playerY,2850,2050)<120){mission=4;cash+=1200;addFeed("CROSS-TOWN COMPLETE +$1200");}
        else if(mission==4 && missionTimer>20){mission=5;cash+=1500;addFeed("NIGHT SHIFT COMPLETE +$1500");}
        else if(mission==5 && wanted>=3 && heat>80){mission=6;cash+=2500;addFeed("HEAT CHECK COMPLETE +$2500");}
        else if(mission==6 && missionKills>=12){cash+=5000;addFeed("CAMPAIGN COMPLETE +$5000"); mission=0; missionKills=0; missionTimer=0;}
        if(missionTimer>60 && mission>0 && mission<5){ missionTimer=0; addFeed("MISSION RESET — FOLLOW THE MARKER"); }
    }

    void shoot(){
        if(shots.size()>8) return;
        Npc best=null; float bd=900;
        for(Npc n:npcs) if(!n.dead){float d=dist(playerX,playerY,n.x,n.y);if(d<bd){bd=d;best=n;}}
        for(Npc n:cops) if(!n.dead){float d=dist(playerX,playerY,n.x,n.y);if(d<bd){bd=d;best=n;}}
        float dx=mxForAim(),dy=myForAim();
        if(best!=null){dx=best.x-playerX;dy=best.y-playerY;float l=(float)Math.hypot(dx,dy);dx/=Math.max(1,l);dy/=Math.max(1,l);}
        else {float l=(float)Math.hypot(dx,dy);if(l<0.1f){dx=1;dy=0;}else{dx/=l;dy/=l;}}
        shots.add(new Shot(playerX+dx*25,playerY+dy*25,dx*1100,dy*1100));
        heat=Math.min(100,heat+5); wanted=Math.min(5,Math.max(1,wanted));
    }
    float mxForAim(){ return joyX==0&&joyY==0?1:joyX; }
    float myForAim(){ return joyX==0&&joyY==0?0:joyY; }
    float dist(float ax,float ay,float bx,float by){return (float)Math.hypot(ax-bx,ay-by);}

    @Override protected void onDraw(Canvas c){
        super.onDraw(c); W=getWidth();H=getHeight(); c.drawColor(Color.rgb(20,24,28));
        if(!started){drawSplash(c);return;}
        float camX=Math.max(0,Math.min(WORLD_W-W,playerX-W*0.5f));
        float camY=Math.max(0,Math.min(WORLD_H-H,playerY-H*0.5f));
        c.save(); c.translate(-camX,-camY);
        drawWorld(c);
        c.restore();
        drawHud(c); drawControls(c);
    }

    void drawWorld(Canvas c){
        p.setStyle(Paint.Style.FILL);
        // water + districts
        p.setColor(Color.rgb(16,50,75));c.drawRect(0,0,WORLD_W,260,p);c.drawRect(0,2140,WORLD_W,WORLD_H,p);
        p.setColor(Color.rgb(63,73,56));c.drawRect(0,260,WORLD_W,2140,p);
        p.setColor(Color.rgb(67,89,105));c.drawRect(0,840,950,2140,p);
        // roads
        p.setColor(Color.rgb(35,37,40));
        for(int x=140;x<WORLD_W;x+=420)c.drawRect(x,260,x+110,2140,p);
        for(int y=330;y<2140;y+=340)c.drawRect(0,y,WORLD_W,y+105,p);
        p.setColor(Color.rgb(115,96,62));
        c.drawRect(980,260,2180,2140,p);
        // road re-cut through center
        p.setColor(Color.rgb(38,40,43)); c.drawRect(1370,260,1495,2140,p); c.drawRect(0,1110,WORLD_W,1225,p);

        for(int x=200;x<WORLD_W;x+=70){p.setColor(Color.rgb(220,197,101));c.drawRect(x,1165,x+34,1170,p);}
        // buildings
        for(Bld b:buildings){
            p.setColor(new int[]{Color.rgb(75,76,81),Color.rgb(108,83,64),Color.rgb(60,67,72),Color.rgb(94,75,99)}[b.t]);
            c.drawRect(b.x,b.y,b.x+b.w,b.y+b.h,p);
            p.setColor(Color.argb(150,235,220,160));
            int cols=Math.max(1,(int)b.w/35), rows=Math.max(1,(int)b.h/35);
            for(int i=0;i<cols;i++)for(int j=0;j<rows;j++) c.drawRect(b.x+8+i*35,b.y+8+j*35,b.x+21+i*35,b.y+20+j*35,p);
        }
        // zone labels
        text(c,"NEON BAY",120,310,32,Color.WHITE); text(c,"SUNSET HEIGHTS",1180,310,32,Color.WHITE); text(c,"LIBERTY BOROUGH",2210,2080,32,Color.WHITE);
        // mission marker
        float tx=missionTargetX(),ty=missionTargetY();
        p.setColor(Color.argb(90,255,255,255));c.drawCircle(tx,ty,48, p);p.setColor(Color.WHITE);p.setStyle(Paint.Style.STROKE);p.setStrokeWidth(6);c.drawCircle(tx,ty,38,p);p.setStyle(Paint.Style.FILL);text(c,"M",tx-11,ty+12,30,Color.DKGRAY);
        // cars
        for(int i=0;i<cars.size();i++){Car v=cars.get(i);if(inCar && i==carIndex)continue;drawCar(c,v);}
        for(Npc n:npcs)if(!n.dead)drawNpc(c,n,false);
        for(Npc n:cops)if(!n.dead)drawNpc(c,n,true);
        // shots
        p.setColor(Color.WHITE);for(Shot s:shots)c.drawCircle(s.x,s.y,6,p);
        // player
        if(!inCar) drawPlayer(c);
        else {p.setColor(Color.argb(80,255,255,255));c.drawCircle(playerX,playerY,33,p);}
    }
    float missionTargetX(){switch(mission){case 0:return 2450;case 1:return 700;case 2:return 2200;case 3:return 2850;case 4:return 1050;case 5:return 1550;default:return 2450;}}
    float missionTargetY(){switch(mission){case 0:return 450;case 1:return 1800;case 2:return 1680;case 3:return 2050;case 4:return 1450;case 5:return 1180;default:return 450;}}

    void drawPlayer(Canvas c){p.setColor(Color.WHITE);c.drawCircle(playerX,playerY,25,p);p.setColor(Color.rgb(20,20,25));c.drawCircle(playerX,playerY-8,9,p);p.setColor(Color.WHITE);c.drawRect(playerX-15,playerY+10,playerX+15,playerY+22,p);}
    void drawNpc(Canvas c,Npc n,boolean cop){p.setColor(cop?Color.rgb(48,92,180):(n.hostile?Color.rgb(150,55,55):Color.rgb(195,150,92)));c.drawCircle(n.x,n.y,21,p);p.setColor(Color.DKGRAY);c.drawRect(n.x-3,n.y-12,n.x+3,n.y+13,p);}
    void drawCar(Canvas c,Car v){int col=new int[]{Color.rgb(200,50,65),Color.rgb(45,110,200),Color.rgb(240,190,70),Color.rgb(70,190,125),Color.rgb(145,80,175)}[v.t];p.setColor(col);c.drawRoundRect(v.x-42,v.y-23,v.x+42,v.y+23,10,10,p);p.setColor(Color.rgb(25,28,31));c.drawRect(v.x-18,v.y-17,v.x+18,v.y+17,p);p.setColor(Color.WHITE);c.drawCircle(v.x-30,v.y-22,7,p);c.drawCircle(v.x+30,v.y-22,7,p);c.drawCircle(v.x-30,v.y+22,7,p);c.drawCircle(v.x+30,v.y+22,7,p);}
    void text(Canvas c,String s,float x,float y,float size,int color){p.setColor(color);p.setTextSize(size);p.setTypeface(Typeface.create(Typeface.DEFAULT,Typeface.BOLD));p.setStyle(Paint.Style.FILL);c.drawText(s,x,y,p);}

    void drawSplash(Canvas c){
        p.setShader(new LinearGradient(0,0,W,H,Color.rgb(6,10,18),Color.rgb(44,20,54),Shader.TileMode.CLAMP));
        c.drawRect(0,0,W,H,p);p.setShader(null);
        text(c,"STREET SOVEREIGN",W*0.09f,H*0.30f,78,Color.WHITE);
        text(c,"AN ORIGINAL OPEN-WORLD CRIME ACTION",W*0.095f,H*0.36f,23,Color.LTGRAY);
        text(c,"NEON BAY  •  SUNSET HEIGHTS  •  LIBERTY BOROUGH",W*0.095f,H*0.42f,25,Color.WHITE);
        p.setColor(Color.WHITE);c.drawRoundRect(W*0.095f,H*0.54f,W*0.37f,H*0.67f,24,24,p);
        text(c,"PLAY",W*0.19f,H*0.625f,38,Color.BLACK);
        text(c,"Created for Hamza",W*0.095f,H*0.83f,20,Color.LTGRAY);
    }
    void drawHud(Canvas c){
        p.setColor(Color.argb(190,0,0,0));c.drawRoundRect(18,16,430,138,20,20,p);
        text(c,"HAMZA",36,50,24,Color.WHITE);text(c,"$"+cash,36,82,28,Color.WHITE);
        p.setColor(Color.DKGRAY);c.drawRoundRect(165,62,390,82,10,10,p);p.setColor(Color.WHITE);c.drawRoundRect(165,62,165+225*health/100f,82,10,10,p);
        text(c,"HP",168,55,16,Color.WHITE); text(c,names[Math.max(0,Math.min(mission,names.length-1))],36,116,18,Color.WHITE);
        if(wanted>0){text(c,"WANTED",W-330,42,18,Color.WHITE);String s="";for(int i=0;i<5;i++)s += i<wanted?"★":"☆";text(c,s,W-330,77,34,Color.WHITE);}
        // minimap
        float mw=190,mh=142;float x=W-mw-20,y=H-mh-18;p.setColor(Color.argb(175,0,0,0));c.drawRoundRect(x,y,x+mw,y+mh,16,16,p);
        p.setColor(Color.rgb(60,65,70));c.drawRect(x+8,y+8,x+mw-8,y+mh-8,p);
        float px=x+8+(playerX/WORLD_W)*(mw-16), py=y+8+(playerY/WORLD_H)*(mh-16);
        p.setColor(Color.WHITE);c.drawCircle(px,py,5,p);float tx=x+8+(missionTargetX()/WORLD_W)*(mw-16),ty=y+8+(missionTargetY()/WORLD_H)*(mh-16);
        p.setStyle(Paint.Style.STROKE);p.setStrokeWidth(2);c.drawCircle(tx,ty,6,p);p.setStyle(Paint.Style.FILL);
        text(c,"MISSION "+(mission+1)+"/7",W-370,110,18,Color.WHITE);
        int yy=155;for(String s:feed){text(c,s,28,yy,17,Color.WHITE);yy+=22;}
        if(inCar){text(c,"DRIVING",W/2f-55,40,18,Color.WHITE);}
        if(mission==5){text(c,"CAUSE CHAOS • REACH 3 WANTED STARS",W*0.5f-230,H*0.13f,22,Color.WHITE);}
    }
    void drawControls(Canvas c){
        float bx=125,by=H-132;p.setColor(Color.argb(70,255,255,255));c.drawCircle(bx,by,88,p);p.setColor(Color.argb(130,255,255,255));c.drawCircle(bx+joyX*45,by+joyY*45,35,p);
        button(c,W-145,H-100,90,"FIRE");button(c,W-270,H-94,66,inCar?"EXIT":"CAR");button(c,W-385,H-95,58,"BRAKE");
    }
    void button(Canvas c,float x,float y,float r,String s){p.setColor(Color.argb(100,255,255,255));c.drawCircle(x,y,r,p);text(c,s,x-p.measureText(s)/2,y+7,17,Color.WHITE);}

    boolean hit(float x,float y,float cx,float cy,float r){return Math.hypot(x-cx,y-cy)<r;}

    @Override public boolean onTouchEvent(android.view.MotionEvent e){
        float x=e.getX(),y=e.getY();int a=e.getActionMasked(),idx=e.getActionIndex(),id=e.getPointerId(idx);
        if(!started && a==MotionEvent.ACTION_DOWN){if(x>W*.08f&&x<W*.43f&&y>H*.5f&&y<H*.72f){started=true;invalidate();}return true;}
        if(a==MotionEvent.ACTION_DOWN||a==MotionEvent.ACTION_POINTER_DOWN){
            if(hit(x,y,W-145,H-100,100)){firePointer=id;firing=true;}
            else if(hit(x,y,W-270,H-94,85)){enterPointer=id;toggleCar();}
            else if(hit(x,y,W-385,H-95,75)){brakePointer=id;}
            else if(x<280&&y>H-260){joyPointer=id;setJoy(x,y);}
        } else if(a==MotionEvent.ACTION_MOVE){
            if(id==joyPointer)setJoy(x,y);
        } else if(a==MotionEvent.ACTION_UP||a==MotionEvent.ACTION_POINTER_UP||a==MotionEvent.ACTION_CANCEL){
            if(id==firePointer){firePointer=-1;firing=false;}
            if(id==enterPointer)enterPointer=-1;if(id==brakePointer)brakePointer=-1;if(id==joyPointer){joyPointer=-1;joyX=joyY=0;}
        }
        return true;
    }

    void setJoy(float x,float y){float cx=125,cy=H-132;joyX=(x-cx)/88f;joyY=(y-cy)/88f;float l=(float)Math.hypot(joyX,joyY);if(l>1){joyX/=l;joyY/=l;}}
    void toggleCar(){
        if(inCar){inCar=false;carIndex=-1;vx=vy=0;addFeed("ON FOOT");return;}
        for(int i=0;i<cars.size();i++)if(dist(playerX,playerY,cars.get(i).x,cars.get(i).y)<90){inCar=true;carIndex=i;addFeed("CAR JACKED — DRIVE");return;}
        addFeed("GET CLOSER TO A CAR");
    }

    @Override public void surfaceCreated(SurfaceHolder h){running=true;loop=new Thread(this);loop.start();}
    @Override public void surfaceDestroyed(SurfaceHolder h){running=false;try{if(loop!=null)loop.join();}catch(Exception ignored){}}
    @Override public void surfaceChanged(SurfaceHolder h,int format,int width,int height){}

    static class Bld{float x,y,w,h;int t;Bld(float x,float y,float w,float h,int t){this.x=x;this.y=y;this.w=w;this.h=h;this.t=t;}}
    class Car{float x,y;int t;boolean special;Car(float x,float y,int t,boolean s){this.x=x;this.y=y;this.t=t;this.special=s;}}
    class Npc{float x,y,hp=100,respawn=8;boolean hostile,dead;float ang=rng.nextFloat()*(float)Math.PI*2;
        Npc(float x,float y,boolean h){this.x=x;this.y=y;hostile=h;}
        void update(float dt){if(dead)return;ang += (rng.nextFloat()-.5f)*dt;float sp=hostile?38:25;x+=Math.cos(ang)*sp*dt;y+=Math.sin(ang)*sp*dt;x=Math.max(20,Math.min(WORLD_W-20,x));y=Math.max(20,Math.min(WORLD_H-20,y));}
        void updateCop(float dt){if(dead)return;float dx=playerX-x,dy=playerY-y,d=(float)Math.hypot(dx,dy);if(d<900){x+=dx/Math.max(1,d)*85*dt;y+=dy/Math.max(1,d)*85*dt;}else update(dt);if(d<150 && rng.nextFloat()<0.9*dt)health-=5;}
    }
    static class Shot{float x,y,dx,dy,life=0.7f;Shot(float x,float y,float dx,float dy){this.x=x;this.y=y;this.dx=dx;this.dy=dy;}}
}
