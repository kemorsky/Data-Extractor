export type LocationData = {
    id: number;
    image?: string;
    editorID: string;
    formKey: string;
    name: string;
    parentLocation: string;
    region: string;
    keywords: string[];
    cells: CellData[];
    locationCategory: string;
    locationType: string;
    status: string;
    relatedQuestName: string;
    relatedQuestUrl: string;
    inhabitingNpcs: NpcData[];
    inhabitants: string[];
    hasQuest: boolean;
    vikunjaLink: string;
    notes: string;
    // wishlist: string;
};

export type CellData = {
  id: number;
  editorID: string;
  formKey: string;
  gridX: number;
  gridY: number;
}

export type NpcData = {
  name: string;
  url: string;
}

export type LocationFilters = {
  query: string;
  statuses: string[];
  keywords: string[];
  locationCategories : string[];
  locationTypes: string[];
  parentLocationsCities: string[];
  parentLocations: string[];
  inhabitants: string[];
  hasQuest: boolean;
};

// export type FilterId =
//     | "locationCategories"
//     | "cities"
//     | "counties"
//     | "locationTypes"
//     | "statuses"
//     | "inhabitants";