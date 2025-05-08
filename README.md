# Guild Receptionist Game - Game Design Document

## 1. Game Overview
- **Title**: Guild Front Desk
- **Genre**: Narrative-driven 2D Simulation / Strategy
- **Platform**: PC (Unity 2D)
- **Target Audience**: Fans of Papers, Please, Beholder, and decision-based games

## 2. Core Concept
You play as the receptionist of a fantasy adventurer's guild.  
Your role is to assign adventurers to appropriate quests based on their personality and skills.  
Decisions affect outcomes, relationships, and story branches.

## 3. Game Loop
1. Start the day
2. Adventurers arrive randomly
3. Inspect their stats and personality
4. Assign them to quests
5. End the day and view results
6. Manage guild reputation, survivors, and special events

## 4. Systems

### 4.1 Adventurer System
- Traits: Name, Class, Personality, Skill Level, History
- Personality affects dialogue and success rates
- Can be procedurally generated

### 4.2 Quest System
- Attributes: Location, Monster Type, Required Roles, Danger Level, Reward
- PCG used to generate most quests
- Some quests are story-locked or event-related

### 4.3 Dialogue System
- Basic dialogue varies based on personality
- LLM Integration planned for dynamic conversation
- Branching outcomes possible

### 4.4 Support System
- Player can "sponsor" an adventurer
- Increases success/reward chances
- Risk: If sponsored adventurer dies → penalty

### 4.5 Story Events
- Fixed events advance main plot (Act 1~3)
- Certain adventurers/events are unique
- Endings vary based on cumulative decisions

## 5. Procedural Content
- Random quest name + danger + condition + reward
- Random adventurers with unique backstories and personalities
- Events or rumors generated daily

## 6. Art & Tone
- Inspired by "Papers, Please", grim/dark yet whimsical tone
- Hand-drawn 2D sprite look
- Static background, animated UI/interactions

## 7. Audio
- Minimalist BGM
- SFX for UI interaction, success/failure sounds, character chatter

## 8. MVP Scope
- Day Loop System
- 5 Predefined adventurers
- 5 Predefined quests
- Random outcome report
- Basic UI and data flow

## 9. Future Scope
- Procedural generation of quests/adventurers
- Dynamic LLM dialogue
- Event-driven story branching
- Multiple endings

