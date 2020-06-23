import { AssociationData, IdAttributes, UpdateRefereeRequest } from "../../apis/referee";
import { RefereeState } from "../../modules/referee/referee";
import { RootState } from "../../rootReducer";
import { DataAttributes } from "../../schemas/getRefereeSchema";
import { PaymentState } from "./types";

export const selectRefereeState = (state: RootState): RefereeState => {
  return {
    certifications: state.referee.certifications,
    error: state.referee.error,
    id: state.referee.id,
    isLoading: state.referee.isLoading,
    locations: state.referee.locations,
    ngbs: state.referee.ngbs,
    referee: state.referee.referee,
    teams: state.referee.teams,
    testAttempts: state.referee.testAttempts,
    testResults: state.referee.testResults,
  };
}

export const initialPaymentState: PaymentState = {
  cancel: false,
  failure: false,
  success: false,
}

export const initialUpdateState = (referee: DataAttributes, locations: IdAttributes[], teams: IdAttributes[]): UpdateRefereeRequest => {
  const ngbData = locations.reduce((data, location): AssociationData => {
    data[location.nationalGoverningBodyId.toString()] = location.associationType
    return data
  }, {} as AssociationData)
  const teamsData = teams.reduce((data, team): AssociationData => {
    data[team.teamId.toString()] = team.associationType
    return data
  }, {} as AssociationData)

  return {
    bio: referee?.bio,
    exportName: referee?.exportName,
    firstName: referee?.firstName,
    lastName: referee?.lastName,
    ngbData,
    pronouns: referee?.pronouns,
    showPronouns: referee?.showPronouns,
    submittedPaymentAt: referee?.submittedPaymentAt,
    teamsData,
  }
}
