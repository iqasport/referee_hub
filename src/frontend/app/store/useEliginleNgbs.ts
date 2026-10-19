import { TeamGroupAffiliation, useGetNgbsQuery } from "./serviceApi";

export const useGetEligibleNgbsQuery = (
  _arg: { groupAffiliations: TeamGroupAffiliation[] },
  options?: Parameters<typeof useGetNgbsQuery>[1]
) => {
  const result = useGetNgbsQuery({ skipPaging: true }, options);

  return {
    ...result,
    data:
      result.data?.items
        ?.map((ngb: { countryCode?: string }) => ngb.countryCode)
        .filter((id: string | undefined): id is string => typeof id === "string") ?? [],
  };
};
