import React, { useEffect, useMemo, useRef, useState } from "react";
import { Canvas, useFrame, useThree } from "@react-three/fiber";
import { Environment, ContactShadows, Html, Preload, Sky, useGLTF } from "@react-three/drei";
import { EffectComposer, Bloom, Vignette, SMAA } from "@react-three/postprocessing";
import * as THREE from "three";
import { SkeletonUtils } from "three-stdlib";

const HUMAN_URL = "https://raw.githubusercontent.com/UMRAM-Bilkent/supine-human-model/main/assets/human.glb";
const SEDAN_URL = "https://raw.githubusercontent.com/Hidencod/tge-assets/main/packs/car-kit/sedan.glb";

const WORLD = 150;
const HUMAN_HEIGHT = 1.72;
const HUMAN_RADIUS = 0.30;
const NPC_SPEED = [0.42, 0.60];
const ROAD_X = [-48, 0, 48];
const ROAD_Z = [-48, 0, 48];

const BUILDINGS = [
  [-64,-64,12,22,12],[-42,-63,14,30,15],[-20,-63,12,18,11],
  [20,-63,15,26,14],[44,-63,12,34,13],[65,-63,13,20,12],
  [-64,-40,11,17,11],[-20,-40,16,28,14],[20,-40,10,20,12],[64,-40,13,24,13],
  [-64,20,13,26,12],[-22,22,14,18,14],[20,20,12,32,12],[62,22,16,24,14],
  [-64,63,13,22,13],[-42,64,12,36,11],[-18,65,15,24,13],[20,62,12,31,12],[48,65,16,19,14],[70,60,10,27,10]
];

const COLORS = ["#26344a","#3b4354","#2d3f4f","#4a3e4f","#354a45","#3a465f"];

const rects = BUILDINGS.map(([x,z,w,h,d]) => ({ x, z, w, h, d }));

function circleHitsRect(x,z,r,box) {
  const px = Math.max(box.x - box.w/2, Math.min(x, box.x + box.w/2));
  const pz = Math.max(box.z - box.d/2, Math.min(z, box.z + box.d/2));
  const dx=x-px, dz=z-pz;
  return dx*dx+dz*dz < r*r;
}

function clampToWorld(pos, pad=2) {
  return { x: THREE.MathUtils.clamp(pos.x,-WORLD/2+pad,WORLD/2-pad), z: THREE.MathUtils.clamp(pos.z,-WORLD/2+pad,WORLD/2-pad) };
}

function capsuleWorldCollision(x,z,r) {
  for (const box of rects) if (circleHitsRect(x,z,r,box)) return true;
  return false;
}

function chooseSidewalkTarget(from, rng) {
  const candidates=[];
  for (const x of ROAD_X) {
    for (const z of ROAD_Z) {
      for (const side of [-1,1]) {
        const c={x:x+side*4.4 + (rng()-.5)*2.2, z:z+(rng()-.5)*11};
        if (!capsuleWorldCollision(c.x,c.z,HUMAN_RADIUS+0.05)) candidates.push(c);
        const d2=(c.x-from.x)**2+(c.z-from.z)**2;
        if (d2>90) c.score=d2;
      }
    }
  }
  candidates.sort((a,b)=>(b.score||0)-(a.score||0));
  return candidates[Math.floor(rng()*Math.min(5,candidates.length))] || {x:from.x+6,z:from.z};
}

function useSharedHuman() {
  const gltf = useGLTF(HUMAN_URL);
  return useMemo(() => {
    const src = gltf.scene;
    const probe = SkeletonUtils.clone(src);
    probe.updateMatrixWorld(true);
    const box = new THREE.Box3().setFromObject(probe);
    const height = Math.max(0.001, box.max.y-box.min.y);
    const scale = HUMAN_HEIGHT / height;
    return { scene: src, scale, clips: gltf.animations || [] };
  }, [gltf]);
}

function Human({ position, rotationY=0, moving=false, tint="#dce7f5", isPlayer=false, paused=false }) {
  const { scene, scale, clips } = useSharedHuman();
  const ref=useRef();
  const mixer=useRef(null);
  const action=useRef(null);

  useEffect(() => {
    if (!ref.current) return;
    const clone=SkeletonUtils.clone(scene);
    clone.scale.setScalar(scale);
    clone.traverse(o => {
      if (o.isMesh) {
        o.castShadow=true; o.receiveShadow=true;
        if (o.material && o.material.clone) {
          o.material=o.material.clone();
          o.material.color.multiply(new THREE.Color(tint));
          o.material.roughness=0.78;
        }
      }
    });
    ref.current.clear();
    ref.current.add(clone);
    mixer.current=new THREE.AnimationMixer(clone);
    const clip=clips.find(c=>/walk|idle|stand/i.test(c.name)) || clips[0];
    if (clip) {
      action.current=mixer.current.clipAction(clip);
      action.current.reset().fadeIn(.18).play();
      action.current.setLoop(THREE.LoopRepeat, Infinity);
      action.current.setEffectiveTimeScale(paused ? 0 : (moving ? 0.46 : 0.24));
    }
    return () => { mixer.current?.stopAllAction(); };
  }, [scene, scale, clips, tint]);

  useFrame((_,dt)=>{
    if (mixer.current && action.current) {
      action.current.setEffectiveTimeScale(paused ? 0 : (moving ? 0.46 : 0.24));
      mixer.current.update(dt);
    }
  });

  return <group ref={ref} position={[position.x,0,position.z]} rotation={[0,rotationY,0]} />;
}

useGLTF.preload(HUMAN_URL);

function CityBuilding({ box, index }) {
  const [x,z,w,h,d]=box;
  return (
    <group position={[x,h/2-0.02,z]}>
      <mesh castShadow receiveShadow>
        <boxGeometry args={[w,h,d]}/>
        <meshStandardMaterial color={COLORS[index%COLORS.length]} roughness={0.72} metalness={0.05}/>
      </mesh>
      <mesh position={[0,0,-d/2-0.012]} receiveShadow>
        <planeGeometry args={[w*.9,h*.84]}/>
        <meshStandardMaterial color="#0c111b" roughness={1}/>
      </mesh>
      {[...Array(5)].map((_,r)=><mesh key={r} position={[0,h*.33-r*h*.15,-d/2-.024]}><planeGeometry args={[w*.7,.05]}/><meshStandardMaterial color="#73a7c4" emissive="#1b4c66" emissiveIntensity={1.5}/></mesh>)}
      <pointLight position={[0,h*.18,-d/2-.4]} intensity={1.6} distance={18} color="#91c9f0"/>
    </group>
  );
}

function Street() {
  const roadMat = new THREE.MeshStandardMaterial({ color:"#111823", roughness:.92, metalness:.03 });
  const sidewalkMat = new THREE.MeshStandardMaterial({ color:"#4b5563", roughness:1 });
  return (
    <group>
      <mesh rotation-x={-Math.PI/2} receiveShadow>
        <planeGeometry args={[WORLD,WORLD]}/>
        <meshStandardMaterial color="#27313b" roughness={1}/>
      </mesh>
      {ROAD_X.map(x=><mesh key={"x"+x} rotation-x={-Math.PI/2} position={[x,.006,0]} receiveShadow><planeGeometry args={[8,WORLD]}/><primitive object={roadMat}/></mesh>)}
      {ROAD_Z.map(z=><mesh key={"z"+z} rotation-x={-Math.PI/2} position={[0,.007,z]} receiveShadow><planeGeometry args={[WORLD,8]}/><primitive object={roadMat}/></mesh>)}
      {ROAD_X.flatMap(x=>[-1,1].map(side=><mesh key={x+side} rotation-x={-Math.PI/2} position={[x+side*5.05,.012,0]} receiveShadow><planeGeometry args={[1.5,WORLD]}/><primitive object={sidewalkMat}/></mesh>))}
      {ROAD_Z.flatMap(z=>[-1,1].map(side=><mesh key={z+side} rotation-x={-Math.PI/2} position={[0,.013,z+side*5.05]} receiveShadow><planeGeometry args={[WORLD,1.5]}/><primitive object={sidewalkMat}/></mesh>))}
      {[...Array(18)].map((_,i)=><mesh key={i} rotation-x={-Math.PI/2} position={[(i-9)*16,.02,0]}><planeGeometry args={[.11,2.2]}/><meshStandardMaterial color="#e9e2b8" emissive="#4b3b19" emissiveIntensity={.2}/></mesh>)}
    </group>
  );
}

function Tree({position,scale=1}) {
  return (
    <group position={position} scale={scale}>
      <mesh position={[0,1.4,0]} castShadow><cylinderGeometry args={[.12,.18,2.8,9]}/><meshStandardMaterial color="#4a3a2d" roughness={1}/></mesh>
      <mesh position={[0,3.15,0]} castShadow><sphereGeometry args={[1.25,14,10]}/><meshStandardMaterial color="#314e3d" roughness={.92}/></mesh>
      <mesh position={[.6,3.55,.3]} castShadow><sphereGeometry args={[.72,12,8]}/><meshStandardMaterial color="#3a5f47" roughness={.92}/></mesh>
    </group>
  );
}

function Car({position, color="#a3b8cb", rotationY=0, moving=false}) {
  const { scene } = useGLTF(SEDAN_URL);
  const clone=useMemo(()=>SkeletonUtils.clone(scene),[scene]);
  const ref=useRef();
  useEffect(()=>{
    clone.scale.setScalar(.84);
    clone.rotation.set(0,Math.PI,0);
    clone.traverse(o=>{if(o.isMesh){o.castShadow=true;o.receiveShadow=true;}});
  },[clone]);
  useFrame((_,dt)=>{
    if(!moving || !ref.current) return;
    ref.current.position.z += 5.0*dt;
    if(ref.current.position.z>72) ref.current.position.z=-72;
  });
  return <group ref={ref} position={position} rotation={[0,rotationY,0]}><primitive object={clone}/></group>;
}
useGLTF.preload(SEDAN_URL);

function Crowd({ playerPos, setPlayerPos }) {
  const rngRef=useRef(Math.random()*100);
  const [people,setPeople]=useState(()=>Array.from({length:14},(_,i)=>{
    const x=-56+(i%7)*18+(Math.random()-.5)*3;
    const z=-28+Math.floor(i/7)*22+(Math.random()-.5)*3;
    return {id:i,x,z,target:{x:x+5,z},speed:NPC_SPEED[0]+Math.random()*(NPC_SPEED[1]-NPC_SPEED[0]),heading:0,tint:["#d8b79f","#90acc7","#b8c997","#cfa9bd","#c4a572","#9cc3c0"][i%6],moving:true};
  }));

  useFrame((_,dt)=>{
    rngRef.current += dt;
    if(rngRef.current < .033) return;
    rngRef.current=0;
    setPeople(prev=>{
      const next=prev.map(p=>({...p}));
      for(let i=0;i<next.length;i++){
        const p=next[i];
        const dx=p.target.x-p.x, dz=p.target.z-p.z;
        let len=Math.hypot(dx,dz);
        if(len<.9){
          p.target=chooseSidewalkTarget({x:p.x,z:p.z},Math.random);
          len=Math.hypot(p.target.x-p.x,p.target.z-p.z);
        }
        let vx=dx/Math.max(len,.001), vz=dz/Math.max(len,.001);
        for(let j=0;j<next.length;j++) if(i!==j){
          const o=next[j], sx=p.x-o.x, sz=p.z-o.z, d=Math.hypot(sx,sz);
          if(d>0.001 && d<1.0){
            const push=(1.0-d)/1.0;
            vx += (sx/d)*push*1.8;
            vz += (sz/d)*push*1.8;
          }
        }
        for(const b of rects){
          const px=Math.max(b.x-b.w/2,Math.min(p.x,b.x+b.w/2));
          const pz=Math.max(b.z-b.d/2,Math.min(p.z,b.z+b.d/2));
          const ox=p.x-px, oz=p.z-pz, d=Math.hypot(ox,oz);
          if(d>0.001 && d<2.2){
            vx += (ox/d)*(2.2-d)*1.8;
            vz += (oz/d)*(2.2-d)*1.8;
          }
        }
        const n=Math.hypot(vx,vz);
        vx/=Math.max(n,.001); vz/=Math.max(n,.001);
        const step=p.speed*dt;
        const nx=p.x+vx*step, nz=p.z+vz*step;
        if(!capsuleWorldCollision(nx,nz,HUMAN_RADIUS)){
          p.x=nx; p.z=nz;
        } else {
          p.target=chooseSidewalkTarget({x:p.x,z:p.z},Math.random);
        }
        p.heading=Math.atan2(vx,vz);
      }
      return next;
    });

    // Player-to-crowd hard separation without physics tunnelling.
    setPlayerPos(prev=>{
      let px=prev.x,pz=prev.z;
      for(const p of people){
        const dx=px-p.x,dz=pz-p.z,d=Math.hypot(dx,dz),min=.66;
        if(d>0.001 && d<min){
          px += dx/d*(min-d); pz += dz/d*(min-d);
        }
      }
      return {...prev,x:px,z:pz};
    });
  });

  return <>
    {people.map(p=><Human key={p.id} position={p} rotationY={p.heading} moving={p.moving} tint={p.tint} paused={paused}/>)}
  </>;
}

function Player({ pos, setPos, look, setLook }) {
  const keys=useRef(new Set());
  useEffect(()=>{
    const down=e=>keys.current.add(e.key.toLowerCase());
    const up=e=>keys.current.delete(e.key.toLowerCase());
    window.addEventListener("keydown",down); window.addEventListener("keyup",up);
    return()=>{window.removeEventListener("keydown",down);window.removeEventListener("keyup",up);};
  },[]);
  useFrame((_,dt)=>{
    const f=(keys.current.has("w")?1:0)-(keys.current.has("s")?1:0);
    const r=(keys.current.has("d")?1:0)-(keys.current.has("a")?1:0);
    if(!f && !r) return;
    const mag=Math.hypot(f,r)||1;
    let nx=pos.x+(r/mag)*3.4*dt, nz=pos.z-(f/mag)*3.4*dt;
    if(!capsuleWorldCollision(nx,nz,HUMAN_RADIUS)) setPos({...pos,x:nx,z:nz});
  });

  useEffect(()=>{
    const onMove=e=>{
      if(e.buttons!==1) return;
      setLook(v=>({yaw:v.yaw-e.movementX*.0022,pitch:THREE.MathUtils.clamp(v.pitch-e.movementY*.0018,-.65,.45)}));
    };
    window.addEventListener("mousemove",onMove);
    return()=>window.removeEventListener("mousemove",onMove);
  },[setLook]);

  return <Human position={pos} rotationY={look.yaw} moving={true} tint="#dfe9f6" isPlayer/>;
}

function FollowCamera({ playerPos, look }) {
  const { camera }=useThree();
  const target=new THREE.Vector3();
  const desired=new THREE.Vector3();
  useFrame((_,dt)=>{
    target.set(playerPos.x,1.25,playerPos.z);
    const dist=6.4, height=2.55;
    const yaw=look.yaw, pitch=look.pitch;
    const horizontal=Math.cos(pitch)*dist;
    desired.set(
      playerPos.x + Math.sin(yaw)*horizontal,
      1.25 + Math.sin(pitch)*dist + height,
      playerPos.z + Math.cos(yaw)*horizontal
    );
    camera.position.lerp(desired,1-Math.exp(-dt*10));
    camera.lookAt(target);
  });
  return null;
}

function World({ playerPos, setPlayerPos, look, setLook }) {
  const cars=useMemo(()=>[
    {x:-48,z:-24,c:"#8795a9",r:0,m:true},
    {x:0,z:28,c:"#d2a34a",r:Math.PI,m:true},
    {x:48,z:12,c:"#7f9c8f",r:Math.PI,m:false},
    {x:-48,z:48,c:"#7e8798",r:0,m:false}
  ],[]);
  const trees=useMemo(()=>Array.from({length:34},(_,i)=>({
    x:-70+(i*19)%140,z:-70+((i*43)%140),s:.7+((i*17)%5)*.09
  })).filter(t=>Math.min(...rects.map(b=>Math.hypot(t.x-b.x,t.z-b.z)))>8),[]);
  return (
    <>
      <color attach="background" args={["#070b13"]}/>
      <Sky sunPosition={[15,8,-20]} turbidity={4.6} rayleigh={1.2} mieCoefficient={.006}/>
      <ambientLight intensity={1.15}/>
      <directionalLight castShadow position={[-18,32,12]} intensity={2.1} shadow-mapSize={[2048,2048]} shadow-camera-top={80} shadow-camera-bottom={-80} shadow-camera-left={-80} shadow-camera-right={80}/>
      <Street/>
      {rects.map((b,i)=><CityBuilding box={b} index={i} key={i}/>)}
      {trees.map((t,i)=><Tree key={i} position={[t.x,0,t.z]} scale={t.s}/>)}
      {cars.map((c,i)=><Car key={i} position={[c.x,.08,c.z]} color={c.c} rotationY={c.r} moving={c.m}/>)}
      <Crowd playerPos={playerPos} setPlayerPos={setPlayerPos}/>
      <Player pos={playerPos} setPos={setPlayerPos} look={look} setLook={setLook}/>
      <FollowCamera playerPos={playerPos} look={look}/>
      <ContactShadows position={[0,.02,0]} scale={WORLD*.85} blur={2.4} opacity={.48} far={18}/>
      <EffectComposer multisampling={0}>
        <SMAA/>
        <Bloom intensity={.65} luminanceThreshold={1.0} mipmapBlur/>
        <Vignette eskil={false} offset={.18} darkness={.68}/>
      </EffectComposer>
      <Preload all/>
    </>
  );
}

export default function App() {
  const [playerPos,setPlayerPos]=useState({x:-34,z:34});
  const [look,setLook]=useState({yaw:Math.PI,pitch:-.12});
  const [ready,setReady]=useState(false);
  const [paused,setPaused]=useState(false);

  useEffect(()=>{
    const timer=setTimeout(()=>setReady(true),1800);
    return()=>clearTimeout(timer);
  },[]);

  return (
    <div className="app">
      <div className="viewport">
        <Canvas shadows camera={{fov:62,near:.05,far:220,position:[-34,3,40]}} dpr={[1,1.7]} gl={{antialias:false,powerPreference:"high-performance"}}>
          <World playerPos={playerPos} setPlayerPos={setPlayerPos} look={look} setLook={setLook}/>
        </Canvas>
      </div>

      <div className="hud">
        <div className="topbar">
          <div>
            <div className="brand">Street Sovereign // React 3D</div>
            <div className="title">San Valora</div>
          </div>
          <div className="stats">
            <div className="pill"><div className="pill-label">Player</div><div className="pill-value">1.72 m</div></div>
            <div className="pill"><div className="pill-label">Crowd</div><div className="pill-value">14 live</div></div>
            <div className="pill"><div className="pill-label">State</div><div className="pill-value">{paused?"Paused":"Live"}</div></div>
          </div>
        </div>
        <div className="bottom">
          <div className="objective"><div className="kicker">Current objective</div><strong>Walk the block. Test the crowd. Explore the city.</strong></div>
          <div className="controls">
            <button className="control" onClick={()=>setPaused(v=>!v)}>{paused?"Resume":"Pause"}</button>
            <button className="control" onClick={()=>setPlayerPos({x:-34,z:34})}>Reset</button>
          </div>
        </div>
      </div>
      <div className="crosshair"/>
      <div className="joystick"><div className="knob"/></div>
      <div className="hint">WASD move · drag mouse to look</div>

      {!ready && <div className="loading"><div className="loading-card"><div className="brand">Street Sovereign</div><div className="title">Loading the district</div><div className="loading-bar"><div/></div></div></div>}
    </div>
  );
}