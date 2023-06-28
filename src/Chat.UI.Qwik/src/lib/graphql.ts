export interface GraphQL<T> {
  data: T;
  errors: any[];
  extensions: any[];
}