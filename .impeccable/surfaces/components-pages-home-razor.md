---
version: 1
slug: "components-pages-home-razor"
primary_target: "Components/Pages/Home.razor"
related_targets: ["Components/Layout/MainLayout.razor"]
---

# App shell and dashboard

Scope: the signed-in app shell (sidebar, top bar) and the Dashboard, with the shared theme carried to
Transactions, Categories, Budgets, Import, Agent Activity, and the sign-in pages. Mode: Operate.

Audience and job: a reviewer (portfolio) or a real user checking where the money went this month, then
acting on flags, the review queue, and budget suggestions. Only real data; no invented widgets.

Constraints: user-pinned look from a supplied mockup (dark indigo/violet glass shell, light gradient KPI tiles
with area history, cash-flow chart with hover readout, donut, list panels). Name stays FinanceAgent.
Bootstrap 5.1 components themed by hand; charts are server-rendered SVG, no chart library.

## Direction contract

THESIS: A night-sky glass console where the month's money reads in three lit tiles and one cash-flow
horizon; refuses the grey admin template with a single accent.

OWN-WORLD: Deep indigo ground lit by violet and blue glows; frosted panels with 1px pale borders; three pastel
tiles (mint income, peach spending, periwinkle net) carrying their own area history; violet primary, mint up,
coral down; Manrope with tabular numerals; consistent 1.75-stroke line icons.

STORY: The visitor sees income, spending and net, sees how the last six months moved, spots what needs
attention, and clicks straight into the flagged or unreviewed transactions.

FIRST VIEWPORT: Sidebar left (logo, six nav items, user card and log out at bottom). Top bar: search over
transactions, Alerts chip with flagged count, avatar. Row 1: three KPI tiles plus month picker and Run agents.
Row 2: cash-flow chart (2/3) beside Needs-attention list (1/3). Row 3: budget ring, spending donut, recent
transactions.

FORM: pinned by the user's reference image, so no concept roll ran (a user-pinned direction beats the roll); seed: none (direction pinned, no roll). Signature move: hovering or focusing a
month on the cash-flow horizon lifts a glass readout of that month's income, spending, and net.

FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance
