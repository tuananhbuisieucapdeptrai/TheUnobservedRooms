# Moth Hack — London 2026 Submission

Paste the following into the Airtable form. Text in `[SQUARE BRACKETS]` must be completed before submission.

## Tell us about yourself / team

**Are you submitting as a team or as an individual?**  
Submitting as an individual person.

**Your name**  
Tuan Anh Bui

**Main contact email address**  
tuananhbui0703@gmail.com

**Main contact Discord handle**  
`[YOUR DISCORD HANDLE — join the Moth Discord first]`

**GitHub handle**  
`[YOUR GITHUB HANDLE]`

**Your occupation**  
Independent game developer and hackathon participant working with Unity, interactive storytelling, and emerging quantum-computing tools.

**Team details**  
Not applicable — individual submission.

**Other**  
This project was also created for the Global Quantum Game Jam 2026. Public profile or portfolio: `[OPTIONAL URL]`.

## Tell us about your project

**Project title**  
The Unobserved Rooms

**Elevator pitch**  
A first-person quantum-horror puzzle where observing uncertain passages consumes coherence, linked controls must be braided into the correct state, and staring at the pursuing Surveyor is the only way to slow it down.

**Select your challenge**  
Quantum game (Eligible for Global Quantum Game Jam)

**Project description — 154 words**  
The Unobserved Rooms is a 10–15 minute first-person quantum-horror puzzle set inside a shifting research facility. Passages begin in uncertain states: observing one makes it traversable, but the measurement consumes coherence. Players must choose which routes to stabilize, recover coherence shards, and avoid falling into darkness while a faceless entity called the Surveyor hunts them. Looking directly at the Surveyor slows it, creating tension between watching the threat and navigating the maze.

The central puzzle consists of spatially separated A and B controls with correlated states. Both stations must be visited and their operations interleaved into the target pattern before the exit becomes available. This interprets the Global Quantum Game Jam theme, quantum BRAIDing, as both a physical route through the facility and a history of linked operations. The project produced a playable Unity game for WebGL and Windows, procedural visual and audio assets, a reproducible run-data format, and an in-game Quantum Run Report.

**Technical description — 90 words**  
Built in Unity 6 with C#, the project uses a validated JSON run definition to construct a 14-room facility at runtime. It is designed around Moth Atlas `labyrinth-v1` for topology, `graph-v1` for the correlated A/B puzzle, and `comet-qrng-v1` for bounded lighting, reward, and anomaly variation. A deterministic cached payload keeps the jam build playable offline, while the result screen exposes engine provenance and the payload hash. The release includes automated validation, four Edit Mode tests, and tested WebGL and Windows build pipelines.

**Which Moth Atlas engines did you use?**  
Select the form options corresponding to:

- `labyrinth-v1`
- `graph-v1`
- `comet-qrng-v1`

Do not select these or claim verified execution until the cached `demo-*` provenance has been replaced with output from completed Atlas jobs.

**QPU or emulation?**  
Emulation/simulation. Do not select QPU: the submitted game does not contain evidence of a quantum-hardware execution.

**Code repository**  
`[PUBLIC GITHUB REPOSITORY URL — REQUIRED]`

Recommended repository description: `Unity quantum-horror puzzle created for Moth Hack London and Global Quantum Game Jam 2026.`

**Demo URL**  
`[PUBLIC ITCH.IO GAME URL]`

**Generative AI usage**  
Yes. Generative AI assisted with code iteration, debugging, documentation, and the creation of original visual textures. All generated material was reviewed, directed, integrated, and tested by the submitter.

**What generative AI tools did you use?**  
Select OpenAI / ChatGPT / Codex and image generation, using the closest available labels in the form.

**Non-Moth APIs**  
No external runtime APIs.

**Non-Moth API details**  
The released game does not call any non-Moth external API at runtime. Unity is used as the game engine, and OpenAI generative tools were used during development as disclosed in the generative-AI section; they are not runtime dependencies of the game.

## Share your project media

**Poster art**  
Upload one landscape hero image between 1:1 and 4:3: `[POSTER FILE]`.

**Demo video**  
`[PUBLIC YOUTUBE OR VIMEO URL — REQUIRED, MAXIMUM 3 MINUTES]`

Suggested video structure:

1. 0:00–0:20 — Title, premise, and objective.
2. 0:20–0:55 — Observe a threshold and explain coherence.
3. 0:55–1:35 — Demonstrate A/B correlation and the braided operation history.
4. 1:35–2:05 — Encounter the Surveyor and show observation slowing it.
5. 2:05–2:35 — Unlock the exit and show the Quantum Run Report.
6. 2:35–2:55 — Explain the three Moth Atlas engine roles and close.

**Additional images**  
Upload up to five images, preferably:

1. Observation threshold and coherence HUD.
2. Control A interaction.
3. Control B and target-state display.
4. Surveyor encounter.
5. Exit or Quantum Run Report.

**Presentation slides**  
Optional. Upload a PDF only if one is prepared.

## Last bits

Tick the eligibility-and-rights confirmation only if the statement is accurate.

Tick permission to show the work if you accept the stated public-display licence.

The teammate-details confirmation is not applicable to an individual entry; follow the form if it nevertheless requires an answer.

“Keep in touch” is optional and does not affect judging.

## Final blockers before pressing Submit

- Replace all `demo-*` provenance entries with genuine completed Moth Atlas job data.
- Publish a public GitHub repository.
- Publish and test the itch.io demo.
- Record and publish a public video no longer than three minutes.
- Add your Discord handle and GitHub handle.
- Upload poster art and the desired screenshots.
- Rotate the Moth API key previously shared in chat; never commit it to the repository or Unity client.
