import { useParams } from "react-router-dom";
import { usePageTitle } from "../../model/page";

export default function Channel() {
  const { id } = useParams<{ id: string }>();
  usePageTitle().value = "Channel " + id;

  return (
    <>
      <h2>Channel {id}</h2>
    </>
  );
}
