import { AuthContext } from "@/components/contexts/AuthContext";
import Link from "next/link";
import { FC, useContext, useEffect, useState } from "react";
import { Card, Col, Container, Row } from "react-bootstrap";

const ProfileInfo: FC = () => {
    const [profile, setProfileInfo] = useState<profileInfo>({ id: null, login: "", name: "", isBlocked:null, bonuses: null, born: null, jobTitle: null, phoneNumber: null, roles: null, gotJob: null, address: null });
    const authContextData = useContext<authContextProps>(AuthContext);

    const orderStoryButtonVisibility:boolean = !(authContextData.isAdmin || authContextData.isKitchenWorker || authContextData.isDeliveryMan);

    useEffect(() => {
        const fetchData = async () => {
            const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getProfileInfo`, {
                credentials: 'include',
                headers: {
                    'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                }, 
            });
            if(resp.ok){
                const profileInfo = await resp.json() as profileInfo;
                setProfileInfo(profileInfo);
            }
            else{
                alert(await resp.text());
            }
        }
        fetchData();
    }, []);

    return (
        <Container className="mt-5">
            <Row className="d-flex justify-content-center">
                <Col md={8}>
                    <Card className="p-3 py-4">
                        <div className="text-center">
                            <img src="https://i.imgur.com/bDLhJiP.jpg" width="100" className="rounded-circle" alt=""/>
                        </div>
                        <div className="text-center mt-3">
                            {/* <span className="bg-secondary p-1 px-4 rounded text-white">{profile.jobTitle}</span> */}
                            <h5 className="mt-2 mb-0">{profile.name}</h5>
                            <span>{profile.login}</span>
                            <div className="px-2 px-md-4 mt-1">
                                <p className="fonts">Consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. </p>
                            </div>
                            <div className="buttons">
                                {orderStoryButtonVisibility && <Link href="/profile/orderStory" className="btn btn-outline-primary px-4">История заказов</Link>}
                                <button className="btn btn-primary px-4 ms-3">Contacts</button>
                            </div>
                        </div>
                    </Card>
                </Col>
            </Row>
        </Container>
    );
}

export default ProfileInfo;