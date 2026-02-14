import Bugsnag from "@bugsnag/js";
import React, { useEffect, useState, lazy, Suspense } from "react";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";

import Avatar from "./components/Avatar";
import Loader from "./components/Loader";
import { useGetCurrentUserQuery } from "./store/serviceApi";

const PUBLIC_ROUTES = ["/privacy"];

const PrivacyPolicy = lazy(() => import("./pages/PrivacyPolicy"));
const RefereeProfile = lazy(() => import("./pages/RefereeProfile"));
const Admin = lazy(() => import("./pages/Admin"));
const Test = lazy(() => import("./pages/Test"));
const StartTest = lazy(() => import("./pages/StartTest"));
const NgbProfile = lazy(() => import("./pages/NgbProfile"));
const ImportWizard = lazy(() => import("./pages/ImportWizard"));
const RefereeTests = lazy(() => import("./pages/RefereeTests"));
const Settings = lazy(() => import("./pages/Settings"));
const Tournament = lazy(() => import("./pages/Tournaments"));
const TournamentDetails = lazy(() => import("./pages/Tournaments/TournamentId"));
const TeamView = lazy(() => import("./pages/TeamView"));
const TeamManagement = lazy(() => import("./pages/TeamManagement"));

const App = () => {
  const [redirectTo, setRedirectTo] = useState<string>();
  const { currentData: currentUser, isError, isLoading } = useGetCurrentUserQuery();
  const roles = currentUser?.roles?.map(r => r.roleType);

  const ownedNgbIds = currentUser ? (() => {
    const ngb = (currentUser?.roles?.filter(r => r.roleType == "NgbAdmin")[0] as any)?.ngb;
    if (typeof ngb === "string") return [ngb];
    else return ngb;
  })() : undefined;

  const getRedirect = () => {
    if (roles.includes("IqaAdmin")) return "/admin";
    if (roles.includes("NgbAdmin")) return `/national_governing_bodies/${ownedNgbIds[0]}`;
    if (roles.includes("Referee")) return `/referees/${currentUser.userId}`;

    return null;
  };

  useEffect(() => {
    const isPublic = PUBLIC_ROUTES.some((route) => window.location.pathname.match(route));

    if (isError && !isPublic) {
      window.location.href = `${window.location.origin}/sign_in`;
    }
  }, [isError]);

  useEffect(() => {
    if (currentUser) {
      setRedirectTo(getRedirect());
    }
  }, [currentUser, roles]);

  if (currentUser) Bugsnag.setUser(currentUser.userId);

  if (isLoading === true) return <Loader />;

  return (
  <Suspense fallback={<Loader />}>
    <BrowserRouter>
      <div>
        <div className="bg-navy-blue text-right text-white py-3 px-10 flex items-center justify-end">
          <p className="flex-shrink mx-8">Management Hub</p>
          { currentUser && <Avatar
            firstName={currentUser.firstName}
            lastName={currentUser.lastName}
            roles={roles}
            userId={currentUser.userId}
            ownedNgbId={ownedNgbIds ? ownedNgbIds[0] : undefined}
            enabledFeatures={/* TODO currentUser?.enabledFeatures*/ undefined}
            />}
        </div>
        <Routes>
          <Route
            path="/"
            element={redirectTo && <Navigate to={redirectTo} replace /> || <></>}
          />
          <Route
            path="/privacy"
            element={<PrivacyPolicy />}
          />
          <Route
            path="/referees/:refereeId"
            element={<RefereeProfile />}
          />
          <Route
            path="/admin"
            element={<Admin />}
          />
          <Route
            path="/admin/tests/:testId"
            element={<Test />}
          />
          <Route
            path="/referees/:refereeId/tests/:testId"
            element={<StartTest />}
          />
          <Route
            path="/national_governing_bodies/:ngbId"
            element={<NgbProfile />}
          />
          <Route
            path="/import/:importScope/:scopeId"
            element={<ImportWizard />}
          />
          <Route
            path="/referees/:refereeId/tests"
            element={<RefereeTests />}
          />
          <Route
          path="/tournaments"
          element={<Tournament/>}
          />
          <Route 
          path="/tournaments/:tournamentId"
          element={<TournamentDetails/>}
          />
          <Route
          path="/teams/:teamId"
          element={<TeamView/>}
          />
          <Route
          path="/teams/:teamId/manage"
          element={<TeamManagement/>}
          />
          {/* TODO {currentUser?.enabledFeatures.includes("i18n") ? (
            <Route
              path="/settings"
              element={<Settings />}
            />
          ) : null} */}
        </Routes>
      </div>
    </BrowserRouter>
  </Suspense>
  );
};

export default App;
