

https://github.com/user-attachments/assets/9d2c445e-63d7-42dd-9f29-727443c44c4e



# Introduction
Welcome to Pixel Arena, a retro-style 2D pixel art top-down shooter where two players battle it out for crowns, the only currency in a dystopian land. In this harsh world, some are born with crowns, enjoying the privileges and power they bring, while others must earn them through fierce combat in the arena. Players must please the higher-ups by proving their prowess in battle, striving to collect as many crowns as possible. With fast-paced action gameplay, and a visually nostalgic aesthetic, Pixel Arena challenges players to outmaneuver and outgun their opponents in a bid for survival and supremacy.

# Key Steps In Implementation
1. Tick Rate :  A 30 ticks per second is setup for ensuring game responsiveness, consistency, latency management, and balanced server resource usage in multiplayer games.
2. Interpolation : Implemented interpolation to estimate and smoothen player positions between received data packets to ensure fluid and continuous gameplay despite network delays.
3. Lag Compensation : lag management adjusts player actions and positions to account for network delays, ensuring fairness and synchronization in multiplayer games.
4. Server Side Rewind : to retroactively assess game events based on past states to accurately process actions and mitigate the effects of latency in multiplayer games.

# How To Use
1. Enter User Name
2. Create or Join Lobby
3. Use Code if Joining Lobby
4. Collect 15 coins before the other player does.
5. Don't let the other player collect coins

# Project Info
- Time frame: 4 Week
- Engine, Tools & Concepts: Unity Engine, Netcode of GameObjets, Unity Replay, Server Side Rewind, Lag Compensation.
- Language & Data Format : C#
- Play the Game : https://doofindog.github.io/PixelArena/

