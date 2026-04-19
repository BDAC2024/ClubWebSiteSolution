# Peg Registration & Allocation Implementation Plan

## Goal
Add peg registration/allocation support across shared entities, SimpleDB repositories, and `WatersController`, following existing repository/controller patterns in this solution.

---

## Scope Requested

1. Add two new entities in `AnglingClubShared`:
   - `PegRegistration`
   - `PegAllocation`
2. Add two new SimpleDB repositories in `AnglingClubWebServices/Data`:
   - `PegRegistrationRepository` (`IdPrefix = "PegRegistration"`)
   - `PegAllocationRepository` (`IdPrefix = "PegAllocation"`)
3. Extend `WatersController` with endpoints:
   - `POST RegisterPeg`
   - `GET GetPegRegistrations`
   - `DELETE DeletePegRegistration`
   - `POST AllocatePeg`
   - `GET GetPegAllocations`

---

## Proposed Implementation Steps

### 1) Create Shared Entities (`AnglingClubShared/Entities`)

#### 1.1 `PegRegistration`
Create `PegRegistration : TableBase` with:
- `string Stretch`
- `string Peg`
- `Season Season`
- `int MembershipNumber`
- `DateTime DateRegistered`
- `string Name` (normal property, not persisted to DB; populated during repository reads)

#### 1.2 `PegAllocation`
Create `PegAllocation : TableBase` with:
- `string Stretch`
- `string Peg`
- `Season Season`
- `int MembershipNumber`
- `DateOnly DateAllocated`
- `string Name` (normal property, not persisted to DB; populated during repository reads)

#### 1.3 `Name` property behavior (confirmed)
Implement `Name` as a **normal non-persisted property** on both entities.
- Do **not** write `Name` to SimpleDB attributes.
- Populate `Name` in repository `Get...` methods **after** all persisted attributes are read and `MembershipNumber` is known.
- Resolve `Name` via member lookup (matching existing member data source conventions).

---

### 2) Add Repository Interfaces (`AnglingClubWebServices/Interfaces`)

Create interfaces:
- `IPegRegistrationRepository`
  - `Task AddOrUpdatePegRegistration(PegRegistration registration)`
  - `Task<List<PegRegistration>> GetPegRegistrations()` (or season-filtered overload)
  - `Task DeletePegRegistration(string id)`
- `IPegAllocationRepository`
  - `Task AddOrUpdatePegAllocation(PegAllocation allocation)`
  - `Task<List<PegAllocation>> GetPegAllocations()` (or season-filtered overload)
  - `Task DeletePegAllocation(string id)` (optional unless intentionally omitted)

Use naming/method style consistent with existing repositories (e.g., `OpenMatchRegistrationRepository`, `WaterRepository`).

---

### 3) Implement Repositories (`AnglingClubWebServices/Data`)

#### 3.1 `PegRegistrationRepository`
- Inherit `RepositoryBase`
- `private const string IdPrefix = "PegRegistration"`
- Implement AddOrUpdate/Get/Delete
- Persist attributes:
  - `Stretch`
  - `Peg`
  - `Season` (integer enum value)
  - `MembershipNumber`
  - `DateRegistered` (ISO string format for stable parse/sort)

#### 3.2 `PegAllocationRepository`
- Inherit `RepositoryBase`
- `private const string IdPrefix = "PegAllocation"`
- Implement AddOrUpdate/Get/(Delete optional)
- Persist attributes:
  - `Stretch`
  - `Peg`
  - `Season`
  - `MembershipNumber`
  - `DateAllocated` (ISO string format for stable parse/sort)

#### 3.3 Parsing & filtering
- Use existing enum parsing conventions for `Season`.
- Return strongly typed entities.
- Controller can filter by season after retrieval unless repository-level query filtering exists.

---

### 4) Wire Up Dependency Injection

Update service registration (likely in `Program.cs`/startup composition root):
- `IPegRegistrationRepository -> PegRegistrationRepository`
- `IPegAllocationRepository -> PegAllocationRepository`

---

### 5) Extend `WatersController`

Inject dependencies:
- `IPegRegistrationRepository`
- `IPegAllocationRepository`
- (Likely needed for `Name` enrichment) `IMemberRepository`

#### 5.1 `POST RegisterPeg`
Parameters: `Stretch`, `Peg`, `Season` (all strings)

Flow:
1. Validate authenticated user exists and has `MembershipNumber`.
2. Parse season string to `Season` enum.
3. Query existing registrations.
4. Check duplicate for **same `Stretch`, `Peg`, `Season`, current user `MembershipNumber`**.
5. If duplicate, return suitable `BadRequest` message.
6. Build `PegRegistration` with current user membership and `DateTime.UtcNow` (or existing project standard).
7. Save via `AddOrUpdatePegRegistration`.
8. Return `Ok` (or created payload).

#### 5.2 `GET GetPegRegistrations`
Parameter: `Season` (string)

Flow:
1. Parse season.
2. Retrieve registrations.
3. Filter by season.
4. Populate/display `Name` from member lookup.
5. Return list.

#### 5.3 `DELETE DeletePegRegistration`
Parameter: `id` (string)

Flow:
1. Validate id.
2. Call repository delete.
3. Return `Ok` / `NotFound` behavior consistent with existing controllers.

#### 5.4 `POST AllocatePeg` (Admin only)
Parameters:
- `Stretch` (string)
- `Peg` (string)
- `MembershipNumber` (int)
- `DateAllocated` (DateOnly)

Flow:
1. Validate user is admin/committee.
2. Validate membership exists (recommended).
3. Set `Season` using an existing extension method that derives season from the passed `DateAllocated`.
4. Check for uniqueness: ensure no existing allocation for same `Stretch`/`Peg`/`DateAllocated`.
5. Build `PegAllocation`.
6. `AddOrUpdatePegAllocation`.
7. Return `Ok`.

#### 5.5 `GET GetPegAllocations`
Parameter: `Season` (string)

Flow:
1. Parse season.
2. Get allocations.
3. Filter by season.
4. Populate/display `Name`.
5. Return list.

---

## Validation & Testing Plan

1. Build solution:
   - `dotnet build`
2. Run relevant tests if available:
   - `dotnet test`
3. Manual API checks (Postman/Swagger):
   - Register peg happy path
   - Register peg duplicate path
   - Get registrations by season
   - Delete registration
   - Allocate peg
   - Get allocations by season
4. Verify SimpleDB persisted attributes and key prefixes.

---

## Deliverables

- `PegRegistrationPlan.md` (this file)
- Implementation can proceed in a follow-up change set.
