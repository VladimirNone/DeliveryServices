import { ChangeEvent, Component, FormEvent } from "react"
import Link from 'next/link'
import { Col, Form, Row, Container, Button } from "react-bootstrap";
import { NextRouter, withRouter } from "next/router";
import { AuthContext } from "@/components/contexts/AuthContext";

type LoginState = {
    login: string,
    password: string,
    success: boolean,
    errors: string[],
}

type WithRouterProps = {
    router: NextRouter
  }

class Login extends Component<WithRouterProps, LoginState> {
    declare context: React.ContextType<typeof AuthContext>
    static contextType = AuthContext; 

    state = {
        login: '',
        password: '',
        success: false,
        errors: [],
    }

    handleInputLogin = (e:ChangeEvent<HTMLInputElement>):void => {
        const inputText = e.target.value;
        this.setState({login:inputText});
    }

    handleInputPassword = (e:ChangeEvent<HTMLInputElement>):void => {
        const inputText = e.target.value;
        this.setState({password:inputText});
    }

    handleSubmit = async (e:FormEvent) => {
        e.preventDefault();

        const response = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/auth/login`, {
            method: "POST",
            credentials: "include",
            headers: {
                'Content-Type': 'application/json;charset=utf-8',
            },
            body: JSON.stringify({login:this.state.login, password:this.state.password})
        });

        if(response.ok){
            this.context.toggleIsAuthed();
            this.props.router.push('/');
        }
        else{
            alert(await response.text());
        }
    }

    render() {
        return (
            <Container fluid={'xl'} className='mt-5 mb-5'>
                <h1>Войти</h1>
                <Form onSubmit={this.handleSubmit}>
                    <h3>Используйте данные от аккаунта dot Food для входа</h3>
                    <div className="text-danger"></div>
                    {/* {this.state.errors.map((error, i) => <p key={i}>{error}</p>)} */}
                    <Row>
                        <Col sm={10} md={6} lg={4}>
                            <Form.Group className={`mb-2`}>
                                <Form.Label>Email</Form.Label>
                                <Form.Control type="email" placeholder="Введите логин" value={this.state.login} onChange={this.handleInputLogin}/>
                                <span className="text-danger"></span>
                            </Form.Group>
                            <Form.Group className={`mb-3`}>
                                <Form.Label>Пароль</Form.Label>
                                <Form.Control type="password" placeholder="Введите пароль" value={this.state.password} onChange={this.handleInputPassword}/>
                                <span className="text-danger"></span>
                            </Form.Group>
                            <Row>
                                <Col xs={6} className="g-0 pe-1 ps-2">
                                    <Button variant="primary" type="submit" className="w-100">
                                        Войти
                                    </Button>
                                </Col>
                                <Col xs={6} className="g-0 ps-1 pe-2">
                                    <Link href={'/signup'} className="w-100">
                                        <Button variant="primary" className="w-100">
                                            Регистрация
                                        </Button>
                                    </Link>
                                </Col>
                            </Row>
                        </Col>
                    </Row>
                </Form>
            </Container>)
    }
};

export default withRouter(Login);